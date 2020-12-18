/*
 * e6502: A complete 6502 CPU emulator.
 * Copyright 2016 Adam Mensch
 */

using System;

namespace KDS.e6502CPU
{
    public enum e6502Type
    {
        CMOS,
        NMOS
    };

    public class e6502
    {
        // Main Register
        public byte A { get; internal set; }

        // Index Registers
        public byte X { get; internal set; }
        public byte Y { get; internal set; }

        // Program Counter
        public ushort PC { get; internal set; }

        // Stack Pointer
        // Memory location is hard coded to 0x01xx
        // Stack is descending (decrement on push, increment on pop)
        // 6502 is an empty stack so SP points to where next value is stored
        public byte SP { get; internal set; }

        // Status Registers (in order bit 7 to 0)
        public bool NF { get; internal set; }    // negative flag (N)
        public bool VF { get; internal set; }    // overflow flag (V)
                                                 // bit 5 is unused
                                                 // bit 4 is the break flag however it is not a physical flag in the CPU
        public bool DF { get; internal set; }    // binary coded decimal flag (D)
        public bool IF { get; internal set; }    // interrupt flag (I)
        public bool ZF { get; internal set; }    // zero flag (Z)
        public bool CF { get; internal set; }    // carry flag (C)

        // Flag for hardware interrupt (IRQ)
        public bool IRQWaiting { get; set; }
        // Flag for non maskable interrupt (NMI)
        public bool NMIWaiting { get; set; }
        // RDY flag
        protected bool RDY { get; set; }

        // List of op codes and their attributes
        private readonly OpCodeTable opCodeTable;

        // The current opcode
        protected OpCodeRecord currentOp;

        private readonly e6502Type CPUType;

        private bool Prefetched = false;
        private int PrefetchedOperand = 0;

        public IBusDevice SystemBus { get; private set; }

        public e6502(IBusDevice bus) : this(bus, e6502Type.NMOS) { }

        public e6502(IBusDevice bus, e6502Type cpuType)
        {
            opCodeTable = new OpCodeTable();

            // Set these on instantiation so they are known values when using this object in testing.
            // Real programs should explicitly load these values before using them.
            A = 0;
            X = 0;
            Y = 0;
            SP = 0;
            PC = 0;
            NF = false;
            VF = false;
            DF = false;
            IF = true;
            ZF = false;
            CF = false;
            NMIWaiting = false;
            IRQWaiting = false;
            CPUType = cpuType;
            SystemBus = bus;
        }

        public void Boot()
        {
            // On reset the addresses 0xfffc and 0xfffd are read and PC is loaded with this value.
            // It is expected that the initial program loaded will have these values set to something.
            // Most 6502 systems contain ROM in the upper region (around 0xe000-0xffff)
            Boot(ReadWord(0xfffc));
        }

        public void Boot(ushort pc)
        {
            PC = pc;
            IF = true;
            NMIWaiting = false;
            IRQWaiting = false;
        }

        public string DasmNextInstruction()
        {
            OpCodeRecord oprec = opCodeTable.OpCodes[SystemBus.Read(PC)];
            if (oprec.Bytes == 3)
                return oprec.Dasm(GetImmWord());
            else
                return oprec.Dasm(GetImmByte());
        }

        /// <summary>
        /// Without executing the instruction determine how many clocks the next instruction will take.
        /// </summary>
        /// <returns>how many clock cycles for the next instruction</returns>
        public virtual int ClocksForNext()
        {
            int clocks = 0;

            if(ProcessInterrupts())
            {
                clocks = 6;
            }

            currentOp = opCodeTable.OpCodes[SystemBus.Read(PC)];
            PrefetchedOperand = GetOperand(currentOp.AddressMode, out bool CrossBoundary);

            clocks += currentOp.Cycles + ClocksForCMOS() + ClocksForBranching();
            if (CrossBoundary) clocks++;

            Prefetched = true;
            return clocks;
        }

        private int ClocksForCMOS()
        {
            int clocks = 0;
            if (CPUType == e6502Type.CMOS)
            {
                switch (currentOp.OpCode)
                {
                    // CMOS fixes a bug in this op code which results in an extra clock cycle
                    case 0x6c:
                        clocks++;
                        break;

                    // extra clock cycle on CMOS in decimal mode
                    case 0x7d:
                    case 0xfd:
                        if (DF) clocks++;
                        break;

                    // On 65C02 (abs,X) takes one less clock cycle (but still add back 1 if page boundary crossed)
                    case 0x1e:
                    case 0x3e:
                    case 0x5e:
                    case 0x7e:
                        clocks--;
                        break;

                }
            }
            return clocks;
        }
        private int ClocksForBranching()
        {
            int clocks = 0;
            // Account for extra cycles if a branch is taken
            switch (currentOp.OpCode)
            {
                // BCC - branch on carry clear
                case 0x90:
                    clocks = PrefetchBranch(!CF);
                    break;
                // BCS - branch on carry set
                case 0xb0:
                    clocks = PrefetchBranch(CF);
                    break;
                // BEQ - branch on zero
                case 0xf0:
                    clocks = PrefetchBranch(ZF);
                    break;
                // BMI - branch on negative
                case 0x30:
                    clocks = PrefetchBranch(NF);
                    break;

                // BNE - branch on non zero
                case 0xd0:
                    clocks = PrefetchBranch(!ZF);
                    break;

                // BPL - branch on non negative
                case 0x10:
                    clocks = PrefetchBranch(!NF);
                    break;

                // BRA - unconditional branch to immediate address
                // NOTE: In OpcodeList.txt the number of clock cycles is one less than the documentation.
                // This is because CheckBranch() adds one when a branch is taken, which in this case is always.
                case 0x80:
                    clocks = PrefetchBranch(true);
                    break;

                // BVC - branch on overflow clear
                case 0x50:
                    clocks = PrefetchBranch(!VF);
                    break;

                // BVS - branch on overflow set
                case 0x70:
                    clocks = PrefetchBranch(VF);
                    break;

            }
            return clocks;
        }
        private int PrefetchBranch(bool flag)
        {
            if (flag)
            {
                // extra cycle if branch destination is a different page than
                // the next instruction
                if ((PC & 0xff00) != ((PC + PrefetchedOperand) & 0xff00))
				{
					return 2;
				}
				else
				{
					return 1;
				}
            }
            return 0;
        }

        // returns # of clock cycles needed to execute the instruction
        public virtual void ExecuteNext()
        {
            if(!Prefetched) ProcessInterrupts();
            ExecuteInstruction();
            Prefetched = false;
        }

        private bool ProcessInterrupts()
        {
            // Check for non maskable interrupt (has higher priority over IRQ)
            if (NMIWaiting)
            {
                DoIRQ(0xfffa);
                NMIWaiting = false;
                return true;
            }
            // Check for hardware interrupt, if enabled
            else if (!IF)
            {
                if (IRQWaiting)
                {
                    DoIRQ(0xfffe);
                    IRQWaiting = false;
                    return true;
                }
            }
            return false;
        }

        private void ExecuteInstruction()
        {
            int result;
            int oper;
            if (Prefetched)
            {
                oper = PrefetchedOperand;
            }
            else
            {
                currentOp = opCodeTable.OpCodes[SystemBus.Read(PC)];
                oper = GetOperand(currentOp.AddressMode, out _);
            }

            switch (currentOp.OpCode)
            {
                // ADC - add memory to accumulator with carry
                // A+M+C -> A,C (NZCV)
                case 0x61:
                case 0x65:
                case 0x69:
                case 0x6d:
                case 0x71:
                case 0x72:
                case 0x75:
                case 0x79:
                case 0x7d:

                    if (DF)
                    {
                        result = CPUMath.HexToBCD(A) + CPUMath.HexToBCD((byte)oper);
                        if (CF) result++;

                        CF = (result > 99);

                        if (result > 99)
                        {
                            result -= 100;
                        }
                        ZF = (result == 0);

                        // convert decimal result to hex BCD result
                        A = CPUMath.BCDToHex(result);

                        // Unlike ZF and CF, the NF flag represents the MSB after conversion
                        // to BCD.
                        NF = (A > 0x7f);
                    }
                    else
                    {
                        ADC((byte)oper);
                    }
                    PC += currentOp.Bytes;
                    break;

                // AND - and memory with accumulator
                // A AND M -> A (NZ)
                case 0x21:
                case 0x25:
                case 0x29:
                case 0x2d:
                case 0x31:
                case 0x32:
                case 0x35:
                case 0x39:
                case 0x3d:
                    result = A & oper;

                    NF = ((result & 0x80) == 0x80);
                    ZF = ((result & 0xff) == 0x00);

                    A = (byte)result;
                    PC += currentOp.Bytes;
                    break;

                // ASL - shift left one bit (NZC)
                // C <- (76543210) <- 0

                case 0x06:
                case 0x16:
                case 0x0a:
                case 0x0e:
                case 0x1e:

                    // shift bit 7 into carry
                    CF = (oper >= 0x80);

                    // shift operand
                    result = oper << 1;

                    NF = ((result & 0x80) == 0x80);
                    ZF = ((result & 0xff) == 0x00);

                    SaveOperand(currentOp.AddressMode, result);
                    PC += currentOp.Bytes;

                    break;

                // BBRx - test bit in memory (no flags)
                // Test the zero page location and branch of the specified bit is clear
                // These instructions are only available on Rockwell and WDC 65C02 chips.
                // Number of clock cycles is the same regardless if the branch is taken.
                case 0x0f:
                case 0x1f:
                case 0x2f:
                case 0x3f:
                case 0x4f:
                case 0x5f:
                case 0x6f:
                case 0x7f:

                    // upper nibble specifies the bit to check
                    byte check_bit = (byte)(currentOp.OpCode >> 4);
                    byte check_value = 0x01;
                    for (int ii = 0; ii < check_bit; ii++)
                    {
                        check_value = (byte)(check_value << 1);
                    }

                    // if the specified bit is 0 then branch
                    byte offset = SystemBus.Read((ushort)(PC + 2));
                    PC += currentOp.Bytes;

                    if ((oper & check_value) == 0x00)
                        PC += offset;

                    break;

                // BBSx - test bit in memory (no flags)
                // Test the zero page location and branch of the specified bit is set
                // These instructions are only available on Rockwell and WDC 65C02 chips.
                // Number of clock cycles is the same regardless if the branch is taken.
                case 0x8f:
                case 0x9f:
                case 0xaf:
                case 0xbf:
                case 0xcf:
                case 0xdf:
                case 0xef:
                case 0xff:

                    // upper nibble specifies the bit to check (but ignore bit 7)
                    check_bit = (byte)((currentOp.OpCode & 0x70) >> 4);
                    check_value = 0x01;
                    for (int ii = 0; ii < check_bit; ii++)
                    {
                        check_value = (byte)(check_value << 1);
                    }

                    // if the specified bit is 1 then branch
                    offset = SystemBus.Read((ushort)(PC + 2));
                    PC += currentOp.Bytes;

                    if ((oper & check_value) == check_value)
                        PC += offset;

                    break;

                // BCC - branch on carry clear
                case 0x90:
                    PC += currentOp.Bytes;
                    CheckBranch(!CF, oper);
                    break;

                // BCS - branch on carry set
                case 0xb0:
                    PC += currentOp.Bytes;
                    CheckBranch(CF, oper);
                    break;

                // BEQ - branch on zero
                case 0xf0:
                    PC += currentOp.Bytes;
                    CheckBranch(ZF, oper);
                    break;

                // BIT - test bits in memory with accumulator (NZV)
                // bits 7 and 6 of oper are transferred to bits 7 and 6 of conditional register (N and V)
                // the zero flag is set to the result of oper AND accumulator
                case 0x24:
                case 0x2c:
                // added by 65C02
                case 0x34:
                case 0x3c:
                case 0x89:
                    result = A & oper;

                    // The WDC programming manual for 65C02 indicates NV are unaffected in immediate mode.
                    // The extended op code test program reflects this.
                    if (currentOp.AddressMode != AddressModes.Immediate)
                    {
                        NF = ((oper & 0x80) == 0x80);
                        VF = ((oper & 0x40) == 0x40);
                    }

                    ZF = ((result & 0xff) == 0x00);

                    PC += currentOp.Bytes;
                    break;

                // BMI - branch on negative
                case 0x30:
                    PC += currentOp.Bytes;
                    CheckBranch(NF, oper);
                    break;

                // BNE - branch on non zero
                case 0xd0:
                    PC += currentOp.Bytes;
                    CheckBranch(!ZF, oper);
                    break;

                // BPL - branch on non negative
                case 0x10:
                    PC += currentOp.Bytes;
                    CheckBranch(!NF, oper);
                    break;

                // BRA - unconditional branch to immediate address
                // NOTE: In OpcodeList.txt the number of clock cycles is one less than the documentation.
                // This is because CheckBranch() adds one when a branch is taken, which in this case is always.
                case 0x80:
                    PC += currentOp.Bytes;
                    CheckBranch(true, oper);
                    break;

                // BRK - force break (I)
                case 0x00:

                    // This is a software interrupt (IRQ).  These events happen in a specific order.

                    // Processor adds two to the current PC
                    PC += 2;

                    // Call IRQ routine
                    DoIRQ(0xfffe, true);

                    // Whether or not the decimal flag is cleared depends on the type of 6502 CPU.
                    // The CMOS 65C02 clears this flag but the NMOS 6502 does not.
                    if (CPUType == e6502Type.CMOS)
                        DF = false;

                    break;
                // BVC - branch on overflow clear
                case 0x50:
                    PC += currentOp.Bytes;
                    CheckBranch(!VF, oper);
                    break;

                // BVS - branch on overflow set
                case 0x70:
                    PC += currentOp.Bytes;
                    CheckBranch(VF, oper);
                    break;

                // CLC - clear carry flag
                case 0x18:
                    CF = false;
                    PC += currentOp.Bytes;
                    break;

                // CLD - clear decimal mode
                case 0xd8:
                    DF = false;
                    PC += currentOp.Bytes;
                    break;

                // CLI - clear interrupt disable bit
                case 0x58:
                    IF = false;
                    PC += currentOp.Bytes;
                    break;

                // CLV - clear overflow flag
                case 0xb8:
                    VF = false;
                    PC += currentOp.Bytes;
                    break;

                // CMP - compare memory with accumulator (NZC)
                // CMP, CPX and CPY are unsigned comparisions
                case 0xc5:
                case 0xc9:
                case 0xc1:
                case 0xcd:
                case 0xd1:
                case 0xd2:
                case 0xd5:
                case 0xd9:
                case 0xdd:

                    byte temp = (byte)(A - oper);

                    CF = A >= (byte)oper;
                    ZF = A == (byte)oper;
                    NF = ((temp & 0x80) == 0x80);

                    PC += currentOp.Bytes;
                    break;

                // CPX - compare memory and X (NZC)
                case 0xe0:
                case 0xe4:
                case 0xec:
                    temp = (byte)(X - oper);

                    CF = X >= (byte)oper;
                    ZF = X == (byte)oper;
                    NF = ((temp & 0x80) == 0x80);

                    PC += currentOp.Bytes;
                    break;

                // CPY - compare memory and Y (NZC)
                case 0xc0:
                case 0xc4:
                case 0xcc:
                    temp = (byte)(Y - oper);

                    CF = Y >= (byte)oper;
                    ZF = Y == (byte)oper;
                    NF = ((temp & 0x80) == 0x80);

                    PC += currentOp.Bytes;
                    break;

                // DEC - decrement memory by 1 (NZ)
                case 0xc6:
                case 0xce:
                case 0xd6:
                case 0xde:
                // added by 65C02
                case 0x3a:
                    result = oper - 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    SaveOperand(currentOp.AddressMode, result);

                    PC += currentOp.Bytes;
                    break;

                // DEX - decrement X by one (NZ)
                case 0xca:
                    result = X - 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    X = (byte)result;
                    PC += currentOp.Bytes;
                    break;

                // DEY - decrement Y by one (NZ)
                case 0x88:
                    result = Y - 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    Y = (byte)result;
                    PC += currentOp.Bytes;
                    break;

                // EOR - XOR memory with accumulator (NZ)
                case 0x41:
                case 0x45:
                case 0x49:
                case 0x4d:
                case 0x51:
                case 0x52:
                case 0x55:
                case 0x59:
                case 0x5d:
                    result = A ^ (byte)oper;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    A = (byte)result;

                    PC += currentOp.Bytes;
                    break;

                // INC - increment memory by 1 (NZ)
                case 0xe6:
                case 0xee:
                case 0xf6:
                case 0xfe:
                // added by 65C02
                case 0x1a:
                    result = oper + 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    SaveOperand(currentOp.AddressMode, result);

                    PC += currentOp.Bytes;
                    break;

                // INX - increment X by one (NZ)
                case 0xe8:
                    result = X + 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    X = (byte)result;
                    PC += currentOp.Bytes;
                    break;

                // INY - increment Y by one (NZ)
                case 0xc8:
                    result = Y + 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    Y = (byte)result;
                    PC += currentOp.Bytes;
                    break;

                // JMP - jump to new location (two byte immediate)
                case 0x4c:
                case 0x6c:
                // added for 65C02
                case 0x7c:

                    if (currentOp.AddressMode == AddressModes.Absolute)
                    {
                        PC = GetImmWord();
                    }
                    else if (currentOp.AddressMode == AddressModes.Indirect)
                    {
                        PC = ReadWord(GetImmWord());
                    }
                    else if (currentOp.AddressMode == AddressModes.AbsoluteX)
                    {
                        PC = ReadWord((ushort)(GetImmWord() + X));
                    }
                    else
                    {
                        throw new InvalidOperationException("This address mode is invalid with the JMP instruction");
                    }

                    break;

                // JSR - jump to new location and save return address
                case 0x20:
                    // documentation says push PC+2 even though this is a 3 byte instruction
                    // When pulled via RTS 1 is added to the result
                    Push((ushort)(PC + 2));
                    PC = GetImmWord();
                    break;

                // LDA - load accumulator with memory (NZ)
                case 0xa1:
                case 0xa5:
                case 0xa9:
                case 0xad:
                case 0xb1:
                case 0xb2:
                case 0xb5:
                case 0xb9:
                case 0xbd:
                    A = (byte)oper;

                    ZF = ((A & 0xff) == 0x00);
                    NF = ((A & 0x80) == 0x80);

                    PC += currentOp.Bytes;
                    break;

                // LDX - load index X with memory (NZ)
                case 0xa2:
                case 0xa6:
                case 0xae:
                case 0xb6:
                case 0xbe:
                    X = (byte)oper;

                    ZF = ((X & 0xff) == 0x00);
                    NF = ((X & 0x80) == 0x80);

                    PC += currentOp.Bytes;
                    break;

                // LDY - load index Y with memory (NZ)
                case 0xa0:
                case 0xa4:
                case 0xac:
                case 0xb4:
                case 0xbc:
                    Y = (byte)oper;

                    ZF = ((Y & 0xff) == 0x00);
                    NF = ((Y & 0x80) == 0x80);

                    PC += currentOp.Bytes;
                    break;


                // LSR - shift right one bit (NZC)
                // 0 -> (76543210) -> C
                case 0x46:
                case 0x4a:
                case 0x4e:
                case 0x56:
                case 0x5e:

                    // shift bit 0 into carry
                    CF = ((oper & 0x01) == 0x01);

                    // shift operand
                    result = oper >> 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    SaveOperand(currentOp.AddressMode, result);

                    PC += currentOp.Bytes;
                    break;

                // NOP - no operation
                case 0xea:
                    PC += currentOp.Bytes;
                    break;

                // ORA - OR memory with accumulator (NZ)
                case 0x01:
                case 0x05:
                case 0x09:
                case 0x0d:
                case 0x11:
                case 0x12:
                case 0x15:
                case 0x19:
                case 0x1d:
                    result = A | (byte)oper;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    A = (byte)result;

                    PC += currentOp.Bytes;
                    break;

                // PHA - push accumulator on stack
                case 0x48:
                    Push(A);
                    PC += currentOp.Bytes;
                    break;

                // PHP - push processor status on stack
                case 0x08:
                    int sr = 0x00;

                    if (NF) sr |= 0x80;
                    if (VF) sr |= 0x40;
                    sr |= 0x20; // bit 5 is always 1
                    sr |= 0x10; // bit 4 is always 1 for PHP
                    if (DF) sr |= 0x08;
                    if (IF) sr |= 0x04;
                    if (ZF) sr |= 0x02;
                    if (CF) sr |= 0x01;

                    Push((byte)sr);
                    PC += currentOp.Bytes;
                    break;

                // PHX - push X on stack
                case 0xda:
                    Push(X);
                    PC += currentOp.Bytes;
                    break;

                // PHY - push Y on stack
                case 0x5a:
                    Push(Y);
                    PC += currentOp.Bytes;
                    break;

                // PLA - pull accumulator from stack (NZ)
                case 0x68:
                    A = PopByte();
                    NF = (A & 0x80) == 0x80;
                    ZF = (A & 0xff) == 0x00;
                    PC += currentOp.Bytes;
                    break;

                // PLP - pull status from stack
                case 0x28:
                    sr = PopByte();

                    NF = (sr & 0x80) == 0x80;
                    VF = (sr & 0x40) == 0x40;
                    DF = (sr & 0x08) == 0x08;
                    IF = (sr & 0x04) == 0x04;
                    ZF = (sr & 0x02) == 0x02;
                    CF = (sr & 0x01) == 0x01;
                    PC += currentOp.Bytes;
                    break;

                // PLX - pull X from stack (NZ)
                case 0xfa:
                    X = PopByte();
                    NF = (X & 0x80) == 0x80;
                    ZF = (X & 0xff) == 0x00;
                    PC += currentOp.Bytes;
                    break;

                // PLY - pull Y from stack (NZ)
                case 0x7a:
                    Y = PopByte();
                    NF = (Y & 0x80) == 0x80;
                    ZF = (Y & 0xff) == 0x00;
                    PC += currentOp.Bytes;
                    break;

                // RMBx - clear bit in memory (no flags)
                // Clear the zero page location of the specified bit
                // These instructions are only available on Rockwell and WDC 65C02 chips.
                case 0x07:
                case 0x17:
                case 0x27:
                case 0x37:
                case 0x47:
                case 0x57:
                case 0x67:
                case 0x77:

                    // upper nibble specifies the bit to check
                    check_bit = (byte)(currentOp.OpCode >> 4);
                    check_value = 0x01;
                    for (int ii = 0; ii < check_bit; ii++)
                    {
                        check_value = (byte)(check_value << 1);
                    }
                    check_value = (byte)~check_value;
                    SaveOperand(currentOp.AddressMode, oper & check_value);
                    PC += currentOp.Bytes;
                    break;

                // SMBx - set bit in memory (no flags)
                // Set the zero page location of the specified bit
                // These instructions are only available on Rockwell and WDC 65C02 chips.
                case 0x87:
                case 0x97:
                case 0xa7:
                case 0xb7:
                case 0xc7:
                case 0xd7:
                case 0xe7:
                case 0xf7:

                    // upper nibble specifies the bit to check (but ignore bit 7)
                    check_bit = (byte)((currentOp.OpCode & 0x70) >> 4);
                    check_value = 0x01;
                    for (int ii = 0; ii < check_bit; ii++)
                    {
                        check_value = (byte)(check_value << 1);
                    }
                    SaveOperand(currentOp.AddressMode, oper | check_value);
                    PC += currentOp.Bytes;
                    break;

                // ROL - rotate left one bit (NZC)
                // C <- 76543210 <- C
                case 0x26:
                case 0x2a:
                case 0x2e:
                case 0x36:
                case 0x3e:

                    // perserve existing cf value
                    bool old_cf = CF;

                    // shift bit 7 into carry flag
                    CF = (oper >= 0x80);

                    // shift operand
                    result = oper << 1;

                    // old carry flag goes to bit zero
                    if (old_cf) result |= 0x01;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);
                    SaveOperand(currentOp.AddressMode, result);

                    PC += currentOp.Bytes;
                    break;

                // ROR - rotate right one bit (NZC)
                // C -> 76543210 -> C
                case 0x66:
                case 0x6a:
                case 0x6e:
                case 0x76:
                case 0x7e:

                    // perserve existing cf value
                    old_cf = CF;

                    // shift bit 0 into carry flag
                    CF = (oper & 0x01) == 0x01;

                    // shift operand
                    result = oper >> 1;

                    // old carry flag goes to bit 7
                    if (old_cf) result |= 0x80;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);
                    SaveOperand(currentOp.AddressMode, result);

                    PC += currentOp.Bytes;
                    break;

                // RTI - return from interrupt
                case 0x40:
                    // pull SR
                    sr = PopByte();

                    NF = (sr & 0x80) == 0x80;
                    VF = (sr & 0x40) == 0x40;
                    DF = (sr & 0x08) == 0x08;
                    IF = (sr & 0x04) == 0x04;
                    ZF = (sr & 0x02) == 0x02;
                    CF = (sr & 0x01) == 0x01;

                    // pull PC
                    PC = PopWord();

                    break;

                // RTS - return from subroutine
                case 0x60:
                    PC = (ushort)(PopWord() + 1);
                    break;

                // SBC - subtract memory from accumulator with borrow (NZCV)
                // A-M-C -> A (NZCV)
                case 0xe1:
                case 0xe5:
                case 0xe9:
                case 0xed:
                case 0xf1:
                case 0xf2:
                case 0xf5:
                case 0xf9:
                case 0xfd:

                    if (DF)
                    {
                        result = CPUMath.HexToBCD(A) - CPUMath.HexToBCD((byte)oper);
                        if (!CF) result--;

                        CF = (result >= 0);

                        // BCD numbers wrap around when subtraction is negative
                        if (result < 0)
                            result += 100;
                        ZF = (result == 0);

                        A = CPUMath.BCDToHex(result);

                        // Unlike ZF and CF, the NF flag represents the MSB after conversion
                        // to BCD.
                        NF = (A > 0x7f);
                    }
                    else
                    {
                        ADC((byte)~oper);
                    }
                    PC += currentOp.Bytes;

                    break;

                // SEC - set carry flag
                case 0x38:
                    CF = true;
                    PC += currentOp.Bytes;
                    break;

                // SED - set decimal mode
                case 0xf8:
                    DF = true;
                    PC += currentOp.Bytes;
                    break;

                // SEI - set interrupt disable bit
                case 0x78:
                    IF = true;
                    PC += currentOp.Bytes;
                    break;

                // STA - store accumulator in memory
                case 0x81:
                case 0x85:
                case 0x8d:
                case 0x91:
                case 0x92:
                case 0x95:
                case 0x99:
                case 0x9d:
                    SaveOperand(currentOp.AddressMode, A);
                    PC += currentOp.Bytes;
                    break;

                // STX - store X in memory
                case 0x86:
                case 0x8e:
                case 0x96:
                    SaveOperand(currentOp.AddressMode, X);
                    PC += currentOp.Bytes;
                    break;

                // STY - store Y in memory
                case 0x84:
                case 0x8c:
                case 0x94:
                    SaveOperand(currentOp.AddressMode, Y);
                    PC += currentOp.Bytes;
                    break;

                // STZ - Store zero
                case 0x64:
                case 0x74:
                case 0x9c:
                case 0x9e:
                    SaveOperand(currentOp.AddressMode, 0);
                    PC += currentOp.Bytes;
                    break;

                // TAX - transfer accumulator to X (NZ)
                case 0xaa:
                    X = A;
                    ZF = ((X & 0xff) == 0x00);
                    NF = ((X & 0x80) == 0x80);
                    PC += currentOp.Bytes;
                    break;

                // TAY - transfer accumulator to Y (NZ)
                case 0xa8:
                    Y = A;
                    ZF = ((Y & 0xff) == 0x00);
                    NF = ((Y & 0x80) == 0x80);
                    PC += currentOp.Bytes;
                    break;

                // TRB - test and reset bits (Z)
                // Perform bitwise AND between accumulator and contents of memory
                case 0x14:
                case 0x1c:
                    SaveOperand(currentOp.AddressMode, ~A & oper);
                    ZF = (A & oper) == 0x00;
                    PC += currentOp.Bytes;
                    break;

                // TSB - test and set bits (Z)
                // Perform bitwise AND between accumulator and contents of memory
                case 0x04:
                case 0x0c:
                    SaveOperand(currentOp.AddressMode, A | oper);
                    ZF = (A & oper) == 0x00;
                    PC += currentOp.Bytes;
                    break;

                // TSX - transfer SP to X (NZ)
                case 0xba:
                    X = SP;
                    ZF = ((X & 0xff) == 0x00);
                    NF = ((X & 0x80) == 0x80);
                    PC += currentOp.Bytes;
                    break;

                // TXA - transfer X to A (NZ)
                case 0x8a:
                    A = X;
                    ZF = ((A & 0xff) == 0x00);
                    NF = ((A & 0x80) == 0x80);
                    PC += currentOp.Bytes;
                    break;

                // TXS - transfer X to SP (no flags -- some online docs are incorrect)
                case 0x9a:
                    SP = X;
                    PC += currentOp.Bytes;
                    break;

                // TYA - transfer Y to A (NZ)
                case 0x98:
                    A = Y;
                    ZF = ((A & 0xff) == 0x00);
                    NF = ((A & 0x80) == 0x80);
                    PC += currentOp.Bytes;
                    break;

                // The original 6502 has undocumented and erratic behavior if
                // undocumented op codes are invoked.  The 65C02 on the other hand
                // are guaranteed to be NOPs although they vary in number of bytes
                // and cycle counts.  These NOPs are listed in the OpcodeList.txt file
                // so the proper number of clock cycles are used.
                //
                // Instructions STP (0xdb) and WAI (0xcb) will reach this case.
                // For now these are treated as a NOP.
                default:
                    PC += currentOp.Bytes;
                    break;
            }
        }

        private int GetOperand(AddressModes mode, out bool CrossBoundary)
        {
            int oper = 0;
            CrossBoundary = false;
            switch (mode)
            {
                // Accumulator mode uses the value in the accumulator
                case AddressModes.Accumulator:
                    oper = A;
                    break;

                // Retrieves the byte at the specified memory location
                case AddressModes.Absolute:
                    oper = SystemBus.Read(GetImmWord());
                    break;

                // Indexed absolute retrieves the byte at the specified memory location
                case AddressModes.AbsoluteX:

                    ushort imm = GetImmWord();
                    ushort result = (ushort)(imm + X);
                    oper = SystemBus.Read(result);
                    if (currentOp.CheckPageBoundary)
                    {
                        CrossBoundary = ((imm & 0xff00) != (result & 0xff00));
                    }
                    break;
                case AddressModes.AbsoluteY:
                    imm = GetImmWord();
                    result = (ushort)(imm + Y);
                    oper = SystemBus.Read(result);
                    if (currentOp.CheckPageBoundary)
                    {
                        CrossBoundary = ((imm & 0xff00) != (result & 0xff00));
                    }
                    break;

                // Immediate mode uses the next byte in the instruction directly.
                case AddressModes.Immediate:
                    oper = GetImmByte();
                    break;

                // Implied or Implicit are single byte instructions that do not use
                // the next bytes for the operand.
                case AddressModes.Implied:
                    break;

                // Indirect mode uses the absolute address to get another address.
                // The immediate word is a memory location from which to retrieve
                // the 16 bit operand.
                case AddressModes.Indirect:
                    oper = ReadWord(GetImmWord());
                    break;

                // The indexed indirect modes uses the immediate byte rather than the
                // immediate word to get the memory location from which to retrieve
                // the 16 bit operand.  This is a combination of ZeroPage indexed and Indirect.
                case AddressModes.XIndirect:

                    /*
                     * 1) fetch immediate byte
                     * 2) add X to the byte
                     * 3) obtain word from this zero page address
                     * 4) return the byte located at the address specified by the word
                     */

                    oper = SystemBus.Read(ReadWord((byte)(GetImmByte() + X)));
                    break;

                // The Indirect Indexed works a bit differently than above.
                // The Y register is added *after* the deferencing instead of before.
                case AddressModes.IndirectY:

                    /*
                        1) Fetch the address (word) at the immediate zero page location
                        2) Add Y to obtain the final target address
                        3)Load the byte at this address
                    */

                    ushort addr = ReadWord(GetImmByte());
                    oper = SystemBus.Read((ushort)(addr + Y));
                    if (currentOp.CheckPageBoundary)
                    {
                        CrossBoundary = ((oper & 0xff00) != (addr & 0xff00));
                    }
                    break;


                // Relative is used for branching, the immediate value is a
                // signed 8 bit value and used to offset the current PC.
                case AddressModes.Relative:
                    oper = CPUMath.SignExtend(GetImmByte());
                    break;

                // Zero Page mode is a fast way of accessing the first 256 bytes of memory.
                // Best programming practice is to place your variables in 0x00-0xff.
                // Retrieve the byte at the indicated memory location.
                case AddressModes.ZeroPage:
                    oper = SystemBus.Read(GetImmByte());
                    break;
                case AddressModes.ZeroPageX:
                    oper = SystemBus.Read((ushort)((GetImmByte() + X) & 0xff));
                    break;
                case AddressModes.ZeroPageY:
                    oper = SystemBus.Read((ushort)((GetImmByte() + Y) & 0xff));
                    break;

                // this mode is from the 65C02 extended set
                // works like ZeroPageY when Y=0
                case AddressModes.ZeroPage0:
                    oper = SystemBus.Read(ReadWord((ushort)(GetImmByte() & 0xff)));
                    break;

                // for this mode do the same thing as ZeroPage
                case AddressModes.BranchExt:
                    oper = SystemBus.Read(GetImmByte());
                    break;
                default:
                    break;
            }
            return oper;
        }

        private void SaveOperand(AddressModes mode, int data)
        {
            switch (mode)
            {
                // Accumulator mode uses the value in the accumulator
                case AddressModes.Accumulator:
                    A = (byte)data;
                    break;

                // Absolute mode retrieves the byte at the indicated memory location
                case AddressModes.Absolute:
                    SystemBus.Write(GetImmWord(), (byte)data);
                    break;
                case AddressModes.AbsoluteX:
                    SystemBus.Write((ushort)(GetImmWord() + X), (byte)data);
                    break;
                case AddressModes.AbsoluteY:
                    SystemBus.Write((ushort)(GetImmWord() + Y), (byte)data);
                    break;

                // Immediate mode uses the next byte in the instruction directly.
                case AddressModes.Immediate:
                    throw new InvalidOperationException("Address mode " + mode.ToString() + " is not valid for this operation");

                // Implied or Implicit are single byte instructions that do not use
                // the next bytes for the operand.
                case AddressModes.Implied:
                    throw new InvalidOperationException("Address mode " + mode.ToString() + " is not valid for this operation");

                // Indirect mode uses the absolute address to get another address.
                // The immediate word is a memory location from which to retrieve
                // the 16 bit operand.
                case AddressModes.Indirect:
                    throw new InvalidOperationException("Address mode " + mode.ToString() + " is not valid for this operation");

                // The indexed indirect modes uses the immediate byte rather than the
                // immediate word to get the memory location from which to retrieve
                // the 16 bit operand.  This is a combination of ZeroPage indexed and Indirect.
                case AddressModes.XIndirect:
                    SystemBus.Write(ReadWord((byte)(GetImmByte() + X)), (byte)data);
                    break;

                // The Indirect Indexed works a bit differently than above.
                // The Y register is added *after* the deferencing instead of before.
                case AddressModes.IndirectY:
                    SystemBus.Write((ushort)(ReadWord(GetImmByte()) + Y), (byte)data);
                    break;

                // Relative is used for branching, the immediate value is a
                // signed 8 bit value and used to offset the current PC.
                case AddressModes.Relative:
                    throw new InvalidOperationException("Address mode " + mode.ToString() + " is not valid for this operation");

                // Zero Page mode is a fast way of accessing the first 256 bytes of memory.
                // Best programming practice is to place your variables in 0x00-0xff.
                // Retrieve the byte at the indicated memory location.
                case AddressModes.ZeroPage:
                    SystemBus.Write(GetImmByte(), (byte)data);
                    break;
                case AddressModes.ZeroPageX:
                    SystemBus.Write((ushort)((GetImmByte() + X) & 0xff), (byte)data);
                    break;
                case AddressModes.ZeroPageY:
                    SystemBus.Write((ushort)((GetImmByte() + Y) & 0xff), (byte)data);
                    break;
                case AddressModes.ZeroPage0:
                    SystemBus.Write(ReadWord((ushort)((GetImmByte()) & 0xff)), (byte)data);
                    break;

                // for this mode do the same thing as ZeroPage
                case AddressModes.BranchExt:
                    SystemBus.Write(GetImmByte(), (byte)data);
                    break;

                default:
                    break;
            }
        }

        private ushort GetImmWord()
        {
            return ReadWord((ushort)(PC + 1));
        }

        private byte GetImmByte()
        {
            return SystemBus.Read((ushort)(PC + 1));
        }

        private ushort ReadWord(ushort address)
        {
            return (ushort)(SystemBus.Read((ushort)(address + 1)) << 8 | SystemBus.Read(address) & 0xffff);
        }

        private void Push(byte data)
        {
            SystemBus.Write((ushort)(0x0100 | SP), data);
            SP--;
        }

        private void Push(ushort data)
        {
            // HI byte is in a higher address, LO byte is in the lower address
            SystemBus.Write((ushort)(0x0100 | SP), (byte)(data >> 8));
            SystemBus.Write((ushort)(0x0100 | (SP - 1)), (byte)(data & 0xff));
            SP -= 2;
        }

        private byte PopByte()
        {
            SP++;
            return SystemBus.Read((ushort)(0x0100 | SP));
        }

        private ushort PopWord()
        {
            // HI byte is in a higher address, LO byte is in the lower address
            SP += 2;
            ushort idx = (ushort)(0x0100 | SP);
            return (ushort)((SystemBus.Read(idx) << 8 | SystemBus.Read((ushort)(idx - 1))) & 0xffff);
        }

        private void ADC(byte oper)
        {
            ushort answer = (ushort)(A + oper);
            if (CF) answer++;

            CF = (answer > 0xff);
            ZF = ((answer & 0xff) == 0x00);
            NF = (answer & 0x80) == 0x80;

            //ushort temp = (ushort)(~(A ^ oper) & (A ^ answer) & 0x80);
            VF = (~(A ^ oper) & (A ^ answer) & 0x80) != 0x00;

            A = (byte)answer;
        }

        private void DoIRQ(ushort vector)
        {
            DoIRQ(vector, false);
        }

        private void DoIRQ(ushort vector, bool isBRK)
        {
            // Push the MSB of the PC
            Push((byte)(PC >> 8));

            // Push the LSB of the PC
            Push((byte)(PC & 0xff));

            // Push the status register
            int sr = 0x00;
            if (NF) sr |= 0x80;
            if (VF) sr |= 0x40;

            sr |= 0x20;             // bit 5 is unused and always 1

            if (isBRK)
                sr |= 0x10;         // software interrupt (BRK) pushes B flag as 1
                                        // hardware interrupt pushes B flag as 0
            if (DF) sr |= 0x08;
            if (IF) sr |= 0x04;
            if (ZF) sr |= 0x02;
            if (CF) sr |= 0x01;

            Push((byte)sr);

            // set interrupt disable flag
            IF = true;

            // On 65C02, IRQ, NMI, and RESET also clear the D flag (but not on BRK) after pushing the status register.
            if (CPUType == e6502Type.CMOS && !isBRK)
                DF = false;

            // load program counter with the interrupt vector
            PC = ReadWord(vector);
        }

        private void CheckBranch(bool flag, int oper)
        {
            if (flag)
            {
                PC += (ushort)oper;
            }

        }
    }
}
