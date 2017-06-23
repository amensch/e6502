using System;
using Untari.Console;

namespace Untari.CPU
{
    public enum e6502Type
    {
        CMOS,
        NMOS
    };

    public class e6502
    {
        // Main Register
        public byte A;

        // Index Registers
        public byte X;
        public byte Y;

        // Program Counter
        public ushort PC;

        // Stack Pointer
        // Memory location is hard coded to 0x01xx
        // Stack is descending (decrement on push, increment on pop)
        // 6502 is an empty stack so SP points to where next value is stored
        public byte SP;

        // Status Registers (in order bit 7 to 0)
        public bool NF;    // negative flag (N)
        public bool VF;    // overflow flag (V)
                           // bit 5 is unused
                           // bit 4 is the break flag however it is not a physical flag in the CPU
        public bool DF;    // binary coded decimal flag (D)
        public bool IF;    // interrupt flag (I)
        public bool ZF;    // zero flag (Z)
        public bool CF;    // carry flag (C)

        // System bus
        private IBus _bus;

        // List of op codes and their attributes
        private OpCodeTable _opCodeTable;

        // The current opcode
        private OpCodeRecord _currentOP;

        // Clock cycles to adjust due to page boundaries being crossed, branches taken, or NMOS/CMOS differences
        private int _extraCycles;

        // Flag for hardware interrupt (IRQ)
        public bool IRQWaiting { get; set; }

        // Flag for non maskable interrupt (NMI)
        public bool NMIWaiting { get; set; }

        public e6502Type _cpuType { get; set; }

        public e6502(e6502Type type, IBus bus)
        {
            _opCodeTable = new OpCodeTable();
            _bus = bus;

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
            _cpuType = type;
        }

        public void Boot()
        {
            // On reset the addresses 0xfffc and 0xfffd are read and PC is loaded with this value.
            // It is expected that the initial program loaded will have these values set to something.
            // Most 6502 systems contain ROM in the upper region (around 0xe000-0xffff)
            PC = GetWordFromMemory(0xfffc);

            // interrupt disabled is set on powerup
            IF = true;

            NMIWaiting = false;
            IRQWaiting = false;
        }

        public void LoadProgram(ushort startingAddress, byte[] program)
        {
            _bus.LoadProgram(startingAddress, program);
            PC = startingAddress;
        }

        public string DasmNextInstruction()
        {
            OpCodeRecord oprec = _opCodeTable.OpCodes[ _bus.GetByte(PC) ];
            if (oprec.Bytes == 3)
                return oprec.Dasm( GetImmWord() );
            else
                return oprec.Dasm( GetImmByte() );
        }

        // returns # of clock cycles needed to execute the instruction
        public int ExecuteNext()
        {
            _extraCycles = 0;

            // Check for non maskable interrupt (has higher priority over IRQ)
            if (NMIWaiting)
            {
                DoIRQ(0xfffa);
                NMIWaiting = false;
                _extraCycles += 6;
            }
            // Check for hardware interrupt, if enabled
            else if (!IF)
            {
                if(IRQWaiting)
                {
                    DoIRQ(0xfffe);
                    IRQWaiting = false;
                    _extraCycles += 6;
                }
            }

            _currentOP = _opCodeTable.OpCodes[_bus.GetByte(PC)];

            ExecuteInstruction();

            return _currentOP.Cycles + _extraCycles;
        }

        private void ExecuteInstruction()
        {
            int result;
            int oper = GetOperand(_currentOP.AddressMode);

            switch (_currentOP.OpCode)
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
                        result = HexToBCD(A) + HexToBCD((byte)oper);
                        if (CF) result++;

                        CF = (result > 99);

                        if (result > 99 )
                        {
                            result -= 100;
                        }
                        ZF = (result == 0);

                        // convert decimal result to hex BCD result
                        A = BCDToHex(result);

                        // Unlike ZF and CF, the NF flag represents the MSB after conversion
                        // to BCD.
                        NF = (A > 0x7f);

                        // extra clock cycle on CMOS in decimal mode
                        if (_cpuType == e6502Type.CMOS)
                            _extraCycles++;
                    }
                    else
                    {
                        ADC((byte)oper);
                    }
                    PC += _currentOP.Bytes;
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
                    PC += _currentOP.Bytes;
                    break;

                // ASL - shift left one bit (NZC)
                // C <- (76543210) <- 0

                case 0x06:
                case 0x16:
                case 0x0a:
                case 0x0e:
                case 0x1e:

                    // On 65C02 (abs,X) takes one less clock cycle (but still add back 1 if page boundary crossed)
                    if (_currentOP.OpCode == 0x1e && _cpuType == e6502Type.CMOS)
                        _extraCycles--;

                    // shift bit 7 into carry
                    CF = (oper >= 0x80);

                    // shift operand
                    result = oper << 1;

                    NF = ((result & 0x80) == 0x80);
                    ZF = ((result & 0xff) == 0x00);

                    SaveOperand(_currentOP.AddressMode, result);
                    PC += _currentOP.Bytes;

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
                    byte check_bit = (byte)(_currentOP.OpCode >> 4);
                    byte check_value = 0x01;
                    for( int ii=0; ii < check_bit; ii++)
                    {
                        check_value = (byte)(check_value << 1);
                    }

                    // if the specified bit is 0 then branch
                    byte offset = _bus.GetByte(PC + 2);
                    PC += _currentOP.Bytes;

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
                    check_bit = (byte)((_currentOP.OpCode & 0x70) >> 4);
                    check_value = 0x01;
                    for (int ii = 0; ii < check_bit; ii++)
                    {
                        check_value = (byte)(check_value << 1);
                    }

                    // if the specified bit is 1 then branch
                    offset = _bus.GetByte(PC + 2);
                    PC += _currentOP.Bytes;

                    if ((oper & check_value) == check_value)
                        PC += offset;

                    break;

                // BCC - branch on carry clear
                case 0x90:
                    PC += _currentOP.Bytes;
                    CheckBranch(!CF, oper);
                    break;

                // BCS - branch on carry set
                case 0xb0:
                    PC += _currentOP.Bytes;
                    CheckBranch(CF, oper);
                    break;

                // BEQ - branch on zero
                case 0xf0:
                    PC += _currentOP.Bytes;
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
                    if (_currentOP.AddressMode != AddressModes.Immediate)
                    {
                        NF = ((oper & 0x80) == 0x80);
                        VF = ((oper & 0x40) == 0x40);
                    }

                    ZF = ((result & 0xff) == 0x00);

                    PC += _currentOP.Bytes;
                    break;

                // BMI - branch on negative
                case 0x30:
                    PC += _currentOP.Bytes;
                    CheckBranch(NF, oper);
                    break;

                // BNE - branch on non zero
                case 0xd0:
                    PC += _currentOP.Bytes;
                    CheckBranch(!ZF, oper);
                    break;

                // BPL - branch on non negative
                case 0x10:
                    PC += _currentOP.Bytes;
                    CheckBranch(!NF, oper);
                    break;

                // BRA - unconditional branch to immediate address
                // NOTE: In OpcodeList.txt the number of clock cycles is one less than the documentation.
                // This is because CheckBranch() adds one when a branch is taken, which in this case is always.
                case 0x80:
                    PC += _currentOP.Bytes;
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
                    if( _cpuType == e6502Type.CMOS )
                        DF = false;

                    break;
                // BVC - branch on overflow clear
                case 0x50:
                    PC += _currentOP.Bytes;
                    CheckBranch(!VF, oper);
                    break;

                // BVS - branch on overflow set
                case 0x70:
                    PC += _currentOP.Bytes;
                    CheckBranch(VF, oper);
                    break;

                // CLC - clear carry flag
                case 0x18:
                    CF = false;
                    PC += _currentOP.Bytes;
                    break;

                // CLD - clear decimal mode
                case 0xd8:
                    DF = false;
                    PC += _currentOP.Bytes;
                    break;

                // CLI - clear interrupt disable bit
                case 0x58:
                    IF = false;
                    PC += _currentOP.Bytes;
                    break;

                // CLV - clear overflow flag
                case 0xb8:
                    VF = false;
                    PC += _currentOP.Bytes;
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

                    PC += _currentOP.Bytes;
                    break;

                // CPX - compare memory and X (NZC)
                case 0xe0:
                case 0xe4:
                case 0xec:
                    temp = (byte)(X - oper);

                    CF = X >= (byte)oper;
                    ZF = X == (byte)oper;
                    NF = ((temp & 0x80) == 0x80);

                    PC += _currentOP.Bytes;
                    break;

                // CPY - compare memory and Y (NZC)
                case 0xc0:
                case 0xc4:
                case 0xcc:
                    temp = (byte)(Y - oper);

                    CF = Y >= (byte)oper;
                    ZF = Y == (byte)oper;
                    NF = ((temp & 0x80) == 0x80);

                    PC += _currentOP.Bytes;
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

                    SaveOperand(_currentOP.AddressMode, result);

                    PC += _currentOP.Bytes;
                    break;

                // DEX - decrement X by one (NZ)
                case 0xca:
                    result = X - 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    X = (byte)result;
                    PC += _currentOP.Bytes;
                    break;

                // DEY - decrement Y by one (NZ)
                case 0x88:
                    result = Y - 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    Y = (byte)result;
                    PC += _currentOP.Bytes;
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

                    PC += _currentOP.Bytes;
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

                    SaveOperand(_currentOP.AddressMode, result);

                    PC += _currentOP.Bytes;
                    break;

                // INX - increment X by one (NZ)
                case 0xe8:
                    result = X + 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    X = (byte)result;
                    PC += _currentOP.Bytes;
                    break;

                // INY - increment Y by one (NZ)
                case 0xc8:
                    result = Y + 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    Y = (byte)result;
                    PC += _currentOP.Bytes;
                    break;

                // JMP - jump to new location (two byte immediate)
                case 0x4c:
                case 0x6c:
                // added for 65C02
                case 0x7c:

                    if (_currentOP.AddressMode == AddressModes.Absolute)
                    {
                        PC = GetImmWord();
                    }
                    else if (_currentOP.AddressMode == AddressModes.Indirect)
                    {
                        PC = (ushort)(GetWordFromMemory(GetImmWord()));
                    }
                    else if( _currentOP.AddressMode == AddressModes.AbsoluteX)
                    {
                        PC = GetWordFromMemory((GetImmWord() + X));
                    }
                    else
                    {
                        throw new InvalidOperationException("This address mode is invalid with the JMP instruction");
                    }

                    // CMOS fixes a bug in this op code which results in an extra clock cycle
                    if (_currentOP.OpCode == 0x6c && _cpuType == e6502Type.CMOS)
                        _extraCycles++;
                    break;

                // JSR - jump to new location and save return address
                case 0x20:
                    // documentation says push PC+2 even though this is a 3 byte instruction
                    // When pulled via RTS 1 is added to the result
                    Push((ushort)(PC+2));  
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

                    PC += _currentOP.Bytes;
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

                    PC += _currentOP.Bytes;
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

                    PC += _currentOP.Bytes;
                    break;


                // LSR - shift right one bit (NZC)
                // 0 -> (76543210) -> C
                case 0x46:
                case 0x4a:
                case 0x4e:
                case 0x56:
                case 0x5e:

                    // On 65C02 (abs,X) takes one less clock cycle (but still add back 1 if page boundary crossed)
                    if (_currentOP.OpCode == 0x5e && _cpuType == e6502Type.CMOS)
                        _extraCycles--;

                    // shift bit 0 into carry
                    CF = ((oper & 0x01) == 0x01);

                    // shift operand
                    result = oper >> 1;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);

                    SaveOperand(_currentOP.AddressMode, result);

                    PC += _currentOP.Bytes;
                    break;

                // NOP - no operation
                case 0xea:
                    PC += _currentOP.Bytes;
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

                    PC += _currentOP.Bytes;
                    break;

                // PHA - push accumulator on stack
                case 0x48:
                    Push(A);
                    PC += _currentOP.Bytes;
                    break;

                // PHP - push processor status on stack
                case 0x08:
                    int sr = 0x00;

                    if (NF) sr = sr | 0x80;
                    if (VF) sr = sr | 0x40;
                    sr = sr | 0x20; // bit 5 is always 1
                    sr = sr | 0x10; // bit 4 is always 1 for PHP
                    if (DF) sr = sr | 0x08;
                    if (IF) sr = sr | 0x04;
                    if (ZF) sr = sr | 0x02;
                    if (CF) sr = sr | 0x01;

                    Push((byte)sr);
                    PC += _currentOP.Bytes;
                    break;

                // PHX - push X on stack
                case 0xda:
                    Push(X);
                    PC += _currentOP.Bytes;
                    break;

                // PHY - push Y on stack
                case 0x5a:
                    Push(Y);
                    PC += _currentOP.Bytes;
                    break;

                // PLA - pull accumulator from stack (NZ)
                case 0x68:
                    A = PopByte();
                    NF = (A & 0x80) == 0x80;
                    ZF = (A & 0xff) == 0x00;
                    PC += _currentOP.Bytes;
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
                    PC += _currentOP.Bytes;
                    break;

                // PLX - pull X from stack (NZ)
                case 0xfa:
                    X = PopByte();
                    NF = (X & 0x80) == 0x80;
                    ZF = (X & 0xff) == 0x00;
                    PC += _currentOP.Bytes;
                    break;

                // PLY - pull Y from stack (NZ)
                case 0x7a:
                    Y = PopByte();
                    NF = (Y & 0x80) == 0x80;
                    ZF = (Y & 0xff) == 0x00;
                    PC += _currentOP.Bytes;
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
                     check_bit = (byte)(_currentOP.OpCode >> 4);
                     check_value = 0x01;
                    for (int ii = 0; ii < check_bit; ii++)
                    {
                        check_value = (byte)(check_value << 1);
                    }
                    check_value = (byte)~check_value;
                    SaveOperand(_currentOP.AddressMode, oper & check_value);
                    PC += _currentOP.Bytes;
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
                    check_bit = (byte)((_currentOP.OpCode & 0x70) >> 4);
                    check_value = 0x01;
                    for (int ii = 0; ii < check_bit; ii++)
                    {
                        check_value = (byte)(check_value << 1);
                    }
                    SaveOperand(_currentOP.AddressMode, oper | check_value);
                    PC += _currentOP.Bytes;
                    break;

                // ROL - rotate left one bit (NZC)
                // C <- 76543210 <- C
                case 0x26:
                case 0x2a:
                case 0x2e:
                case 0x36:
                case 0x3e:

                    // On 65C02 (abs,X) takes one less clock cycle (but still add back 1 if page boundary crossed)
                    if (_currentOP.OpCode == 0x3e && _cpuType == e6502Type.CMOS)
                        _extraCycles--;

                    // perserve existing cf value
                    bool old_cf = CF;

                    // shift bit 7 into carry flag
                    CF = (oper >= 0x80);

                    // shift operand
                    result = oper << 1;

                    // old carry flag goes to bit zero
                    if (old_cf) result = result | 0x01;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);
                    SaveOperand(_currentOP.AddressMode, result);

                    PC += _currentOP.Bytes;
                    break;

                // ROR - rotate right one bit (NZC)
                // C -> 76543210 -> C
                case 0x66:
                case 0x6a:
                case 0x6e:
                case 0x76:
                case 0x7e:

                    // On 65C02 (abs,X) takes one less clock cycle (but still add back 1 if page boundary crossed)
                    if (_currentOP.OpCode == 0x7e && _cpuType == e6502Type.CMOS)
                        _extraCycles--;

                    // perserve existing cf value
                    old_cf = CF;

                    // shift bit 0 into carry flag
                    CF = (oper & 0x01) == 0x01;

                    // shift operand
                    result = oper >> 1;

                    // old carry flag goes to bit 7
                    if (old_cf) result = result | 0x80;

                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);
                    SaveOperand(_currentOP.AddressMode, result);

                    PC += _currentOP.Bytes;
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
                        result = HexToBCD(A) - HexToBCD((byte)oper);
                        if (!CF) result--;

                        CF = (result >= 0);

                        // BCD numbers wrap around when subtraction is negative
                        if (result < 0)
                            result += 100;
                        ZF = (result == 0);

                        A = BCDToHex(result);
                        
                        // Unlike ZF and CF, the NF flag represents the MSB after conversion
                        // to BCD.
                        NF = (A > 0x7f);

                        // extra clock cycle on CMOS in decimal mode
                        if (_cpuType == e6502Type.CMOS)
                            _extraCycles++;
                    }
                    else
                    {
                        ADC((byte)~oper);
                    }
                    PC += _currentOP.Bytes;

                    break;

                // SEC - set carry flag
                case 0x38:
                    CF = true;
                    PC += _currentOP.Bytes;
                    break;

                // SED - set decimal mode
                case 0xf8:
                    DF = true;
                    PC += _currentOP.Bytes;
                    break;

                // SEI - set interrupt disable bit
                case 0x78:
                    IF = true;
                    PC += _currentOP.Bytes;
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
                    SaveOperand(_currentOP.AddressMode, A);
                    PC += _currentOP.Bytes;
                    break;

                // STX - store X in memory
                case 0x86:
                case 0x8e:
                case 0x96:
                    SaveOperand(_currentOP.AddressMode, X);
                    PC += _currentOP.Bytes;
                    break;

                // STY - store Y in memory
                case 0x84:
                case 0x8c:
                case 0x94:
                    SaveOperand(_currentOP.AddressMode, Y);
                    PC += _currentOP.Bytes;
                    break;

                // STZ - Store zero
                case 0x64:
                case 0x74:
                case 0x9c:
                case 0x9e:
                    SaveOperand(_currentOP.AddressMode, 0);
                    PC += _currentOP.Bytes;
                    break;

                // TAX - transfer accumulator to X (NZ)
                case 0xaa:
                    X = A;
                    ZF = ((X & 0xff) == 0x00);
                    NF = ((X & 0x80) == 0x80);
                    PC += _currentOP.Bytes;
                    break;

                // TAY - transfer accumulator to Y (NZ)
                case 0xa8:
                    Y = A;
                    ZF = ((Y & 0xff) == 0x00);
                    NF = ((Y & 0x80) == 0x80);
                    PC += _currentOP.Bytes;
                    break;

                // TRB - test and reset bits (Z)
                // Perform bitwise AND between accumulator and contents of memory
                case 0x14:
                case 0x1c:
                    SaveOperand(_currentOP.AddressMode, ~A & oper);
                    ZF = (A & oper) == 0x00;
                    PC += _currentOP.Bytes;
                    break;

                // TSB - test and set bits (Z)
                // Perform bitwise AND between accumulator and contents of memory
                case 0x04:
                case 0x0c:
                    SaveOperand(_currentOP.AddressMode, A | oper);
                    ZF = (A & oper) == 0x00;
                    PC += _currentOP.Bytes;
                    break;

                // TSX - transfer SP to X (NZ)
                case 0xba:
                    X = SP;
                    ZF = ((X & 0xff) == 0x00);
                    NF = ((X & 0x80) == 0x80);
                    PC += _currentOP.Bytes;
                    break;

                // TXA - transfer X to A (NZ)
                case 0x8a:
                    A = X;
                    ZF = ((A & 0xff) == 0x00);
                    NF = ((A & 0x80) == 0x80);
                    PC += _currentOP.Bytes;
                    break;

                // TXS - transfer X to SP (no flags -- some online docs are incorrect)
                case 0x9a:
                    SP = X;
                    PC += _currentOP.Bytes;
                    break;

                // TYA - transfer Y to A (NZ)
                case 0x98:
                    A = Y;
                    ZF = ((A & 0xff) == 0x00);
                    NF = ((A & 0x80) == 0x80);
                    PC += _currentOP.Bytes;
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
                    PC += _currentOP.Bytes;
                    break;
            }
        }

        private int GetOperand(AddressModes mode)
        {
            int oper = 0;
            switch (mode)
            {
                // Accumulator mode uses the value in the accumulator
                case AddressModes.Accumulator:
                    oper = A;
                    break;

                // Retrieves the byte at the specified memory location
                case AddressModes.Absolute:             
                    oper = _bus.GetByte(GetImmWord());
                    break;

                // Indexed absolute retrieves the byte at the specified memory location
                case AddressModes.AbsoluteX:

                    ushort imm = GetImmWord();
                    ushort result = (ushort)(imm + X);

                    if (_currentOP.CheckPageBoundary)
                    {
                        if ((imm & 0xff00) != (result & 0xff00)) _extraCycles++;
                    }
                    oper = _bus.GetByte(result);
                    break;
                case AddressModes.AbsoluteY:
                    imm = GetImmWord();
                    result = (ushort)(imm + Y);

                    if (_currentOP.CheckPageBoundary)
                    {
                        if ((imm & 0xff00) != (result & 0xff00)) _extraCycles++;
                    }
                    oper = _bus.GetByte(result); break;

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
                    oper = GetWordFromMemory(GetImmWord());
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

                    oper = _bus.GetByte(GetWordFromMemory( (byte)(GetImmByte() + X)));
                    break;

                // The Indirect Indexed works a bit differently than above.
                // The Y register is added *after* the deferencing instead of before.
                case AddressModes.IndirectY:

                    /*
                        1) Fetch the address (word) at the immediate zero page location
                        2) Add Y to obtain the final target address
                        3)Load the byte at this address
                    */

                    ushort addr = GetWordFromMemory(GetImmByte());
                    oper = _bus.GetByte(addr + Y);

                    if (_currentOP.CheckPageBoundary)
                    {
                        if ((oper & 0xff00) != (addr & 0xff00)) _extraCycles++;
                    }
                    break;


                // Relative is used for branching, the immediate value is a
                // signed 8 bit value and used to offset the current PC.
                case AddressModes.Relative:
                    oper = SignExtend(GetImmByte());
                    break;
                    
                // Zero Page mode is a fast way of accessing the first 256 bytes of memory.
                // Best programming practice is to place your variables in 0x00-0xff.
                // Retrieve the byte at the indicated memory location.
                case AddressModes.ZeroPage:
                    oper = _bus.GetByte(GetImmByte());
                    break;
                case AddressModes.ZeroPageX:
                    oper = _bus.GetByte((GetImmByte() + X) & 0xff);
                    break;
                case AddressModes.ZeroPageY:
                    oper = _bus.GetByte((GetImmByte() + Y) & 0xff);
                    break;

                // this mode is from the 65C02 extended set
                // works like ZeroPageY when Y=0
                case AddressModes.ZeroPage0:
                    oper = _bus.GetByte(GetWordFromMemory((GetImmByte()) & 0xff));
                    break;

                // for this mode do the same thing as ZeroPage
                case AddressModes.BranchExt:
                    oper = _bus.GetByte(GetImmByte());
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
                    _bus.WriteByte(GetImmWord(), (byte)data);
                    break;
                case AddressModes.AbsoluteX:
                    _bus.WriteByte(GetImmWord() + X, (byte)data);
                    break;
                case AddressModes.AbsoluteY:
                    _bus.WriteByte(GetImmWord() + Y, (byte)data);
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
                    _bus.WriteByte(GetWordFromMemory((byte)(GetImmByte() + X)), (byte)data);
                    break;

                // The Indirect Indexed works a bit differently than above.
                // The Y register is added *after* the deferencing instead of before.
                case AddressModes.IndirectY:
                    _bus.WriteByte(GetWordFromMemory(GetImmByte()) + Y, (byte)data);
                    break;

                // Relative is used for branching, the immediate value is a
                // signed 8 bit value and used to offset the current PC.
                case AddressModes.Relative:
                    throw new InvalidOperationException("Address mode " + mode.ToString() + " is not valid for this operation");

                // Zero Page mode is a fast way of accessing the first 256 bytes of memory.
                // Best programming practice is to place your variables in 0x00-0xff.
                // Retrieve the byte at the indicated memory location.
                case AddressModes.ZeroPage:
                    _bus.WriteByte(GetImmByte(), (byte)data);
                    break;
                case AddressModes.ZeroPageX:
                    _bus.WriteByte((GetImmByte() + X) & 0xff, (byte)data);
                    break;
                case AddressModes.ZeroPageY:
                    _bus.WriteByte((GetImmByte() + Y) & 0xff, (byte)data);
                    break;
                case AddressModes.ZeroPage0:
                    _bus.WriteByte(GetWordFromMemory((GetImmByte()) & 0xff), (byte)data);
                    break;

                // for this mode do the same thing as ZeroPage
                case AddressModes.BranchExt:
                    _bus.WriteByte(GetImmByte(), (byte)data);
                    break;

                default:
                    break;
            }
        }

        private ushort GetWordFromMemory(int address)
        {
            return (ushort)((_bus.GetByte(address + 1) << 8) | (_bus.GetByte(address) & 0xffff));
        }

        private ushort GetImmWord()
        {
            return (ushort)((_bus.GetByte(PC + 2) << 8) | (_bus.GetByte(PC + 1) & 0xffff));
        }

        private byte GetImmByte()
        {
            return _bus.GetByte(PC + 1);
        }

        private int SignExtend(int num)
        {
            if (num < 0x80)
                return num;
            else
                return (0xff << 8 | num) & 0xffff;
        }

        private void Push(byte data)
        {
            _bus.WriteByte(0x0100 | SP, data);
            SP--;
        }

        private void Push(ushort data)
        {
            // HI byte is in a higher address, LO byte is in the lower address
            _bus.WriteByte(0x0100 | SP, (byte)(data >> 8));
            _bus.WriteByte(0x0100 | (SP-1), (byte)(data & 0xff));
            SP -= 2;
        }

        private byte PopByte()
        {
            SP++;
            return _bus.GetByte(0x0100 | SP);
        }
        
        private ushort PopWord()
        {
            // HI byte is in a higher address, LO byte is in the lower address
            SP += 2;
            ushort idx = (ushort)(0x0100 | SP);
            return (ushort)((_bus.GetByte(idx) << 8 | _bus.GetByte(idx-1)) & 0xffff);
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

        private int HexToBCD(byte oper)
        {
            // validate input is valid packed BCD 
            if (oper > 0x99)
                throw new InvalidOperationException("Invalid BCD number: " + oper.ToString("X2"));
            if ((oper & 0x0f) > 0x09)
                throw new InvalidOperationException("Invalid BCD number: " + oper.ToString("X2"));

            return ((oper >> 4) * 10) + (oper & 0x0f);
        }

        private byte BCDToHex(int result)
        {
            if (result > 0xff)
                throw new InvalidOperationException("Invalid BCD to hex number: " + result.ToString());

            if (result <= 9)
                return (byte)result;
            else
                return (byte)(((result / 10) << 4) + (result % 10));

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
            if (NF) sr = sr | 0x80;
            if (VF) sr = sr | 0x40;

            sr = sr | 0x20;             // bit 5 is unused and always 1

            if(isBRK)
                sr = sr | 0x10;         // software interrupt (BRK) pushes B flag as 1
                                        // hardware interrupt pushes B flag as 0
            if (DF) sr = sr | 0x08;
            if (IF) sr = sr | 0x04;
            if (ZF) sr = sr | 0x02;
            if (CF) sr = sr | 0x01;

            Push((byte)sr);

            // set interrupt disable flag
            IF = true;

            // On 65C02, IRQ, NMI, and RESET also clear the D flag (but not on BRK) after pushing the status register.
            if (_cpuType == e6502Type.CMOS && !isBRK)
                DF = false;

            // load program counter with the interrupt vector
            PC = GetWordFromMemory(vector);
        }
        
        private void CheckBranch(bool flag, int oper)
        {
            if (flag)
            {
                // extra cycle on branch taken
                _extraCycles++;

                // extra cycle if branch destination is a different page than
                // the next instruction
                if ((PC & 0xff00) != ((PC + oper) & 0xff00))
                    _extraCycles++;

                PC += (ushort)oper;
            }

        }
    }
}
