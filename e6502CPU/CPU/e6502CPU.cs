using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e6502CPU
{
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
        public bool BF;    // breakpoint flag (B)
        public bool DF;    // binary coded decimal flag (D)
        public bool IF;  // interrupt flag (I)
        public bool ZF;    // zero flag (Z)
        public bool CF;    // carry flag (C)

        // RAM - 16 bit address bus means 64KB of addressable memory
        public byte[] memory;

        // List of op codes and their attributes
        private OpCodeTable _opCodeTable;

        // The current opcode
        private OpCodeRecord _currentOP;

        // The current operand (for debugging)
        private int _operand;

        // Clock cycles to add due to page boundaries being crossed
        private int _extraCycles;

        public e6502()
        {
            memory = new byte[0x10000];
            _opCodeTable = new OpCodeTable();

            // Set these on instantiation so they are known values when using this object in testing.
            // Real programs should explicitly load these values before using them.
            A = 0;
            X = 0;
            Y = 0;
            SP = 0;
            PC = 0;
            NF = false;
            VF = false;
            BF = false;
            DF = false;
            IF = false;
            ZF = false;
            CF = false;
        }

        public void Boot()
        {
            // On reset the addresses 0xfffc and 0xfffd are read and PC is loaded with this value.
            // It is expected that the initial program loaded will have these values set to something.
            // Most 6502 systems contain ROM in the upper region (around 0xe000-0xffff)
            PC = (ushort)((memory[0xfffd] << 8 | memory[0xfffc]) & 0xffff);

        }

        public void LoadProgram(ushort startingAddress, byte[] program)
        {
            program.CopyTo(memory, startingAddress);
            PC = startingAddress;
        }

        public string DasmNextInstruction()
        {
            // NOTE: This method does not alter the program counter
            OpCodeRecord oprec = _opCodeTable.OpCodes[ memory[PC] ];
            if (oprec.Bytes == 3)
                return oprec.Dasm( GetImmWord() );
            else
                return oprec.Dasm( GetImmByte() );
        }

        // returns # of clock cycles needed to execute the instruction
        public int ExecuteNext()
        {
            _currentOP = _opCodeTable.OpCodes[memory[PC]];
            _extraCycles = 0;

            // for now just turn off the flag
            //if (IF) IF = false;
            
            ExecuteInstruction();

            return _currentOP.Cycles + _extraCycles;
        }

        private void ExecuteInstruction()
        {
            // Get operand (if applicable)
            _operand = GetOperand(_currentOP.AddressMode);

            // Temporarily store results so flag calculations can be made
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
                case 0x75:
                case 0x79:
                case 0x7d:
                    result = A + oper;
                    if (CF) result++;

                    CF = ((result & 0x100) == 0x100);
                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);
                    VF = ((result ^ A) & (result ^ oper) & 0x80) == 0x80;

                    A = (byte)result;
                    PC += _currentOP.Bytes;
                    break;

                // AND - and memory with accumulator
                // A AND M -> A (NZ)
                case 0x21:
                case 0x25:
                case 0x29:
                case 0x2d:
                case 0x31:
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

                    // shift bit 7 into carry
                    CF = (oper >= 0x80);

                    // shift operand
                    result = oper << 1;

                    NF = ((result & 0x80) == 0x80);
                    ZF = ((result & 0xff) == 0x00);

                    SaveOperand(_currentOP.AddressMode, result);
                    PC += _currentOP.Bytes;

                    break;

                // BCC - branch on carry clear
                case 0x90:
                    PC += _currentOP.Bytes;
                    if (!CF)
                    {
                        if (_currentOP.CheckBranchPage)
                        {
                            if ((PC & 0xff00) == ((PC + oper) & 0xff00))
                                _extraCycles = 1;
                            else if ((PC & 0xff00) != ((PC + oper) & 0xff00))
                                _extraCycles = 2;
                        }
                        PC += (ushort)oper; 
                    }
                    break;

                // BCS - branch on carry set
                case 0xb0:
                    PC += _currentOP.Bytes;
                    if (CF)
                    {
                        if (_currentOP.CheckBranchPage)
                        {
                            if ((PC & 0xff00) == ((PC + oper) & 0xff00))
                                _extraCycles = 1;
                            else if ((PC & 0xff00) != ((PC + oper) & 0xff00))
                                _extraCycles = 2;
                        }
                        PC += (ushort)oper;
                    }
                    break;

                // BEQ - branch on zero
                case 0xf0:
                    PC += _currentOP.Bytes;
                    if (ZF)
                    {
                        if (_currentOP.CheckBranchPage)
                        {
                            if ((PC & 0xff00) == ((PC + oper) & 0xff00))
                                _extraCycles = 1;
                            else if ((PC & 0xff00) != ((PC + oper) & 0xff00))
                                _extraCycles = 2;
                        }
                        PC += (ushort)oper;
                    }
                    break;

                // BIT - test bits in memory with accumulator (NZV)
                // bits 7 and 6 of oper are transferred to bits 7 and 6 of conditional register (N and V)
                // the zero flag is set to the result of oper AND accumulator
                case 0x24:
                case 0x2c:
                    result = A & oper;

                    NF = ((oper & 0x80) == 0x80);
                    VF = ((oper & 0x40) == 0x40);
                    ZF = ((result & 0xff) == 0x00);

                    PC += _currentOP.Bytes;
                    break;

                // BMI - branch on negative
                case 0x30:
                    PC += _currentOP.Bytes;
                    if (NF)
                    {
                        if (_currentOP.CheckBranchPage)
                        {
                            if ((PC & 0xff00) == ((PC + oper) & 0xff00))
                                _extraCycles = 1;
                            else if ((PC & 0xff00) != ((PC + oper) & 0xff00))
                                _extraCycles = 2;
                        }
                        PC += (ushort)oper;
                    }
                    break;

                // BNE - branch on non zero
                case 0xd0:
                    PC += _currentOP.Bytes;
                    if (!ZF)
                    {
                        if (_currentOP.CheckBranchPage)
                        {
                            if ((PC & 0xff00) == ((PC + oper) & 0xff00))
                                _extraCycles = 1;
                            else if ((PC & 0xff00) != ((PC + oper) & 0xff00))
                                _extraCycles = 2;
                        }
                        PC += (ushort)oper;
                    }
                    break;

                // BPL - branch on non negative
                case 0x10:
                    PC += _currentOP.Bytes;
                    if (!NF)
                    {
                        if (_currentOP.CheckBranchPage)
                        {
                            if ((PC & 0xff00) == ((PC + oper) & 0xff00))
                                _extraCycles = 1;
                            else if ((PC & 0xff00) != ((PC + oper) & 0xff00))
                                _extraCycles = 2;
                        }
                        PC += (ushort)oper;
                    }
                    break;

                // BRK - force break (I)
                case 0x00:

                    // This is a software interrupt (IRQ).  These events happen in a specific order.

                    // Processor adds two to the current PC
                    PC += 2;

                    // Push the MSB of the PC
                    Push((byte)(PC >> 8));

                    // Push the LSB of the PC
                    Push((byte)(PC & 0xff));

                    // Push the status register
                    int sr = 0x00;
                    if (NF) sr = sr | 0x80;
                    if (VF) sr = sr | 0x40;
                    sr = sr | 0x20;
                    sr = sr | 0x10;
                    if (DF) sr = sr | 0x08;
                    if (IF) sr = sr | 0x04;
                    if (ZF) sr = sr | 0x02;
                    if (CF) sr = sr | 0x01;

                    Push((byte)sr);

                    // set interrupt flag
                    IF = true;

                    // load program counter with proper interrupt vector
                    // BRK/IRQ has LSB at $FFFE and MSB at $FFFF
                    PC = (ushort)(memory[0xffff] << 8 | memory[0xfffe]);

                    break;
                // BVC - branch on overflow clear
                case 0x50:
                    PC += _currentOP.Bytes;
                    if (!VF)
                    {
                        if (_currentOP.CheckBranchPage)
                        {
                            if ((PC & 0xff00) == ((PC + oper) & 0xff00))
                                _extraCycles = 1;
                            else if ((PC & 0xff00) != ((PC + oper) & 0xff00))
                                _extraCycles = 2;
                        }
                        PC += (ushort)oper;
                    }
                    break;

                // BVS - branch on overflow set
                case 0x70:
                    PC += _currentOP.Bytes;
                    if (VF)
                    {
                        if (_currentOP.CheckBranchPage)
                        {
                            if ((PC & 0xff00) == ((PC + oper) & 0xff00))
                                _extraCycles = 1;
                            else if ((PC & 0xff00) != ((PC + oper) & 0xff00))
                                _extraCycles = 2;
                        }
                        PC += (ushort)oper;
                    }
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

                    if( _currentOP.AddressMode == AddressModes.Absolute)
                    {
                        PC = GetImmWord();
                    }
                    else if( _currentOP.AddressMode == AddressModes.Indirect)
                    {
                        PC = (ushort)(GetWordFromMemory(GetImmWord()));
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
                    Push((ushort)(PC+2));  
                    PC = GetImmWord();
                    break;

                // LDA - load accumulator with memory (NZ)
                case 0xa1:
                case 0xa5:
                case 0xa9:
                case 0xad:
                case 0xb1:
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


                // LSR - shift right one bit (ZC)
                // 0 -> (76543210) -> C
                case 0x46:
                case 0x4a:
                case 0x4e:
                case 0x56:
                case 0x5e:

                    // shift bit 0 into carry
                    CF = (oper >= 0x01);

                    // shift operand
                    result = oper >> 1;

                    ZF = ((result & 0xff) == 0x00);
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
                    sr = 0x00;

                    if (NF) sr = sr | 0x80;
                    if (VF) sr = sr | 0x40;
                    sr = sr | 0x20; // bit 5 is always on
                    sr = sr | 0x10; // bit 4 is always on for PHP
                    if (DF) sr = sr | 0x08;
                    if (IF) sr = sr | 0x04;
                    if (ZF) sr = sr | 0x02;
                    if (CF) sr = sr | 0x01;

                    Push((byte)sr);
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
                    BF = (sr & 0x10) == 0x10;
                    DF = (sr & 0x08) == 0x08;
                    IF = (sr & 0x04) == 0x04;
                    ZF = (sr & 0x02) == 0x02;
                    CF = (sr & 0x01) == 0x01;
                    PC += _currentOP.Bytes;
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
                    BF = (sr & 0x10) == 0x10;
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
                case 0xf5:
                case 0xf9:
                case 0xfd:
                    result = A - oper;
                    if (!CF) result--;

                    CF = ((result & 0x100) == 0x100);
                    ZF = ((result & 0xff) == 0x00);
                    NF = ((result & 0x80) == 0x80);
                    VF = ((result ^ A) & (result ^ oper) & 0x80) == 0x80;

                    A = (byte)result;
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

                default:
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
                    oper = memory[ GetImmWord() ];
                    break;

                // Indexed absolute retrieves the byte at the specified memory location
                case AddressModes.AbsoluteX:

                    ushort imm = GetImmWord();
                    ushort result = (ushort)(imm + X);

                    if (_currentOP.CheckPageBoundary)
                    {
                        if ((imm & 0xff00) != (result & 0xff00)) _extraCycles = 1;
                    }
                    oper = memory[ result ];
                    break;
                case AddressModes.AbsoluteY:
                    imm = GetImmWord();
                    result = (ushort)(imm + Y);

                    if (_currentOP.CheckPageBoundary)
                    {
                        if ((imm & 0xff00) != (result & 0xff00)) _extraCycles = 1;
                    }
                    oper = memory[result]; break;

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
                    oper = memory[GetImmByte() + X] & 0xff;
                    break;

                // The Indirect Indexed works a bit differently than above.
                // The Y register is added *after* the deferencing instead of before.
                case AddressModes.IndirectY:

                    imm = GetImmByte();
                    if (_currentOP.CheckPageBoundary)
                    {
                        if (imm == 0xff) _extraCycles = 1;
                    }
                    oper = memory[imm] + Y;
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
                    oper = memory[GetImmByte()];
                    break;
                case AddressModes.ZeroPageX:
                    oper = memory[(GetImmByte() + X) & 0xff];
                    break;
                case AddressModes.ZeroPageY:
                    oper = memory[(GetImmByte() + Y) & 0xff];
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
                    memory[GetImmWord()] = (byte)data;
                    break;
                case AddressModes.AbsoluteX:
                    memory[GetImmWord() + X] = (byte)data;
                    break;
                case AddressModes.AbsoluteY:
                    memory[GetImmWord() + Y] = (byte)data;
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
                    throw new InvalidOperationException("Address mode " + mode.ToString() + " is not valid for this operation");

                // The Indirect Indexed works a bit differently than above.
                // The Y register is added *after* the deferencing instead of before.
                case AddressModes.IndirectY:
                    throw new InvalidOperationException("Address mode " + mode.ToString() + " is not valid for this operation");

                // Relative is used for branching, the immediate value is a
                // signed 8 bit value and used to offset the current PC.
                case AddressModes.Relative:
                    throw new InvalidOperationException("Address mode " + mode.ToString() + " is not valid for this operation");

                // Zero Page mode is a fast way of accessing the first 256 bytes of memory.
                // Best programming practice is to place your variables in 0x00-0xff.
                // Retrieve the byte at the indicated memory location.
                case AddressModes.ZeroPage:
                    memory[GetImmByte()] = (byte)data;
                    break;
                case AddressModes.ZeroPageX:
                    memory[(GetImmByte() + X) & 0xff] = (byte)data;
                    break;
                case AddressModes.ZeroPageY:
                    memory[(GetImmByte() + Y) & 0xff] = (byte)data;
                    break;
                default:
                    break;
            }
        }

        private int GetWordFromMemory(int address)
        {
            return (memory[address + 1] << 8 | memory[address]) & 0xffff;
        }

        private ushort GetImmWord()
        {
            return (ushort)((memory[PC + 2] << 8 | memory[PC + 1]) & 0xffff);
        }

        private byte GetImmByte()
        {
            return memory[PC + 1];
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
            memory[(0x0100 | SP)] = data;
            SP--;
        }

        private void Push(ushort data)
        {
            // HI byte is in a higher address, LO byte is in the lower address
            memory[(0x0100 | SP)] = (byte)(data >> 8);
            memory[(0x0100 | (SP-1))] = (byte)(data & 0xff);
            SP -= 2;
        }

        private byte PopByte()
        {
            SP++;
            return memory[(0x0100 | SP)];
        }
        
        private ushort PopWord()
        {
            // HI byte is in a higher address, LO byte is in the lower address
            SP += 2;
            ushort idx = (ushort)(0x0100 | SP);
            return (ushort)((memory[idx] << 8 | memory[idx-1]) & 0xffff);
        }
    }
}
