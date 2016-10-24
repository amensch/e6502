using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e6502CPU
{
    public class e6502CPU
    {
        // Main Register
        private byte A;

        // Index Registers
        private byte X;
        private byte Y;

        // Progream Counter
        private ushort PC;

        // Stack Pointer
        // Memory location is hard coded to 0x01xx
        // Stack is descending (decrement on push, increment on pop)
        // 6502 is an empty stack so SP points to where next value is stored
        private byte SP;

        // Status Registers (in order bit 7 to 0)
        private bool nf;    // negative flag (N)
        private bool of;    // overflow flag (V)
        // bit 5 not used
        private bool bf;    // breakpoint flag (B)
        private bool df;    // binary coded decimal flag (D)
        private bool intf;  // interrupt flag (I)
        private bool zf;    // zero flag (Z)
        private bool cf;    // carry flag (C)

        // RAM - 16 bit address bus means 64KB of addressable memory
        private byte[] memory;

        // List of op codes and their attributes
        private OpCodeTable _opCodeTable;

        // The current opcode
        private OpCodeRecord _currentOP;

        // The current operand (for debugging)
        private int _operand;

        // Clock cycles to add due to page boundaries being crossed
        private int _extraCycles;

        public e6502CPU()
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
            nf = false;
            of = false;
            bf = false;
            df = false;
            intf = false;
            zf = false;
            cf = false;
        }

        public void Boot()
        {
            // On reset the addresses 0xfffc and 0xfffd are read and PC is loaded with this value.
            // It is expected that the initial program loaded will have these values set to something.
            // Most 6502 systems contain ROM in the upper region (around 0xe000-0xffff)
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
            PC += _currentOP.Bytes;
            _extraCycles = 0;

            ExecuteInstruction();

            return _currentOP.Cycles + _extraCycles;
        }

        private void ExecuteInstruction()
        {
            // Get operand (if applicable)
            _operand = GetOperand(_currentOP.AddressMode);

            // Temporarily store results so flag calculations can be made
            int result;
            int oper = memory[GetOperand(_currentOP.AddressMode)];

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
                    if (cf) result++;

                    cf = (result > 0xff);
                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);
                    of = ((result ^ A) & (result ^ oper) & 0x80) == 0x80;

                    A = (byte)result;
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

                    nf = ((result & 0x80) == 0x80);
                    zf = ((result & 0xff) == 0x00);

                    A = (byte)result;
                    break;

                // ASL - shift left one bit (NZC)
                // C <- (76543210) <- 0

                case 0x06:
                case 0x16:
                case 0x0a:
                case 0x0e:
                case 0x1e:

                    // shift bit 7 into carry
                    cf = (oper >= 0x80);

                    // shift operand
                    result = oper << 1;

                    nf = ((result & 0x80) == 0x80);
                    zf = ((result & 0xff) == 0x00);

                    SaveOperand(_currentOP.AddressMode, result);

                    break;

                // BCC - branch on carry clear
                case 0x90:
                    if (!cf)
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
                    if (cf)
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
                    if (zf)
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

                    nf = ((oper & 0x80) == 0x80);
                    of = ((oper & 0x40) == 0x40);
                    zf = ((result & 0xff) == 0x00);

                    break;

                // BMI - branch on negative
                case 0x30:
                    if (nf)
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
                    if (!zf)
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
                    if (!nf)
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
                    // push PC
                    Push(PC);

                    // push SR
                    int sr = 0x00;

                    if (nf) sr = sr & 0x80;
                    if (of) sr = sr & 0x40;
                    // no bit 5
                    if (bf) sr = sr & 0x10;
                    if (df) sr = sr & 0x08;
                    if (intf) sr = sr & 0x04;
                    if (zf) sr = sr & 0x02;
                    if (cf) sr = sr & 0x01;

                    Push((byte)sr);

                    // set interrupt flag
                    intf = true;
                    break;

                // BVC - branch on overflow clear
                case 0x50:
                    if (!of)
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
                    if (of)
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
                    cf = false;
                    break;

                // CLD - clear decimal mode
                case 0xd8:
                    df = false;
                    break;

                // CLI - clear interrupt disable bit
                case 0x58:
                    intf = false;
                    break;

                // CLV - clear overflow flag
                case 0xb8:
                    of = false;
                    break;

                // CMP - compare memory with accumulator (NZC)
                case 0xc5:
                case 0xc9:
                case 0xc1:
                case 0xcd:
                case 0xd1:
                case 0xd5:
                case 0xd9:
                case 0xdd:
                    result = A - (byte)oper;

                    cf = (result > 0xff);
                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    break;

                // CPX - compare memory and X (NZC)
                case 0xe0:
                case 0xe4:
                case 0xec:
                    result = X - (byte)oper;

                    cf = (result > 0xff);
                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    break;

                // CPY - compare memory and Y (NZC)
                case 0xc0:
                case 0xc4:
                case 0xcc:
                    result = Y - (byte)oper;

                    cf = (result > 0xff);
                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    break;

                // DEC - decrement memory by 1 (NZ)
                case 0xc6:
                case 0xce:
                case 0xd6:
                case 0xde:
                    result = oper - 1;

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    SaveOperand(_currentOP.AddressMode, result);

                    break;

                // DEX - decrement X by one (NZ)
                case 0xca:
                    result = X - 1;

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    X = (byte)result;
                    break;

                // DEY - decrement Y by one (NZ)
                case 0x88:
                    result = Y - 1;

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    Y = (byte)result;
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

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    A = (byte)result;

                    break;

                // INC - increment memory by 1 (NZ)
                case 0xe6:
                case 0xee:
                case 0xf6:
                case 0xfe:
                    result = oper + 1;

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    SaveOperand(_currentOP.AddressMode, result);

                    break;

                // INX - increment X by one (NZ)
                case 0xe8:
                    result = X + 1;

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    X = (byte)result;
                    break;

                // INY - increment Y by one (NZ)
                case 0xc8:
                    result = Y + 1;

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    Y = (byte)result;
                    break;

                // JMP - jump to new location (two byte immediate)
                case 0x4c:
                case 0x6c:
                    PC =(ushort) oper;
                    break;

                // JSR - jump to new location and save return address
                case 0x20:
                    Push(PC);
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

                    zf = ((A & 0xff) == 0x00);
                    nf = ((A & 0x80) == 0x80);

                    break;

                // LDX - load index X with memory (NZ)
                case 0xa2:
                case 0xa6:
                case 0xae:
                case 0xb6:
                case 0xbe:
                    X = (byte)oper;

                    zf = ((X & 0xff) == 0x00);
                    nf = ((X & 0x80) == 0x80);

                    break;

                // LDY - load index Y with memory (NZ)
                case 0xa0:
                case 0xa4:
                case 0xac:
                case 0xb4:
                case 0xbc:
                    Y = (byte)oper;

                    zf = ((Y & 0xff) == 0x00);
                    nf = ((Y & 0x80) == 0x80);

                    break;


                // LSR - shift right one bit (ZC)
                // 0 -> (76543210) -> C
                case 0x46:
                case 0x4a:
                case 0x4e:
                case 0x56:
                case 0x5e:

                    // shift bit 0 into carry
                    cf = (oper >= 0x01);

                    // shift operand
                    result = oper >> 1;

                    zf = ((result & 0xff) == 0x00);
                    SaveOperand(_currentOP.AddressMode, result);

                    break;

                // NOP - no operation
                case 0xea:
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

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);

                    A = (byte)result;

                    break;

                // PHA - push accumulator on stack
                case 0x48:
                    Push(A);
                    break;

                // PHP - push processor status on stack
                case 0x08:
                    sr = 0x00;

                    if (nf) sr = sr & 0x80;
                    if (of) sr = sr & 0x40;
                    // no bit 5
                    if (bf) sr = sr & 0x10;
                    if (df) sr = sr & 0x08;
                    if (intf) sr = sr & 0x04;
                    if (zf) sr = sr & 0x02;
                    if (cf) sr = sr & 0x01;

                    Push((byte)sr);
                    break;

                // PLA - pull accumulator from stack (NZ)
                case 0x68:
                    A = PopByte();
                    nf = (A & 0x80) == 0x80;
                    zf = (A & 0x02) == 0x02;
                    break;

                // PLP - pull status from stack
                case 0x28:
                    sr = PopByte();

                    nf = (sr & 0x80) == 0x80;
                    of = (sr & 0x40) == 0x40;
                    bf = (sr & 0x10) == 0x10;
                    df = (sr & 0x08) == 0x08;
                    intf = (sr & 0x04) == 0x04;
                    zf = (sr & 0x02) == 0x02;
                    cf = (sr & 0x01) == 0x01;
                    break;

                // ROL - rotate left one bit (NZC)
                // C <- 76543210 <- C
                case 0x26:
                case 0x2a:
                case 0x2e:
                case 0x36:
                case 0x3e:

                    // perserve existing cf value
                    bool old_cf = cf;

                    // shift bit 7 into carry flag
                    cf = (oper >= 0x80);

                    // shift operand
                    result = oper << 1;

                    // old carry flag goes to bit zero
                    if (old_cf) result = result | 0x01;

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);
                    SaveOperand(_currentOP.AddressMode, result);

                    break;

                // ROR - rotate right one bit (NZC)
                // C -> 76543210 -> C
                case 0x66:
                case 0x6a:
                case 0x6e:
                case 0x76:
                case 0x7e:

                    // perserve existing cf value
                    old_cf = cf;

                    // shift bit 0 into carry flag
                    cf = (oper & 0x01) == 0x01;

                    // shift operand
                    result = oper >> 1;

                    // old carry flag goes to bit 7
                    if (old_cf) result = result | 0x80;

                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);
                    SaveOperand(_currentOP.AddressMode, result);

                    break;

                // RTI - return from interrupt
                case 0x40:
                    // pull SR
                    sr = PopByte();

                    nf = (sr & 0x80) == 0x80;
                    of = (sr & 0x40) == 0x40;
                    bf = (sr & 0x10) == 0x10;
                    df = (sr & 0x08) == 0x08;
                    intf = (sr & 0x04) == 0x04;
                    zf = (sr & 0x02) == 0x02;
                    cf = (sr & 0x01) == 0x01;

                    // pull PC
                    PC = PopWord();

                    break;

                // RTS - return from subroutine
                case 0x60:
                    PC = PopWord();
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
                    if (cf) result--;

                    cf = (result > 0xff);
                    zf = ((result & 0xff) == 0x00);
                    nf = ((result & 0x80) == 0x80);
                    of = ((result ^ A) & (result ^ oper) & 0x80) == 0x80;

                    A = (byte)result;
                    break;

                // SEC - set carry flag
                case 0x38:
                    cf = true;
                    break;

                // SED - set decimal mode
                case 0xf8:
                    df = true;
                    break;

                // SEI - set interrupt disable bit
                case 0x78:
                    intf = true;
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
                    break;

                // STX - store X in memory
                case 0x86:
                case 0x8e:
                case 0x96:
                    SaveOperand(_currentOP.AddressMode, X);
                    break;

                // STY - store Y in memory
                case 0x84:
                case 0x8c:
                case 0x94:
                    SaveOperand(_currentOP.AddressMode, Y);
                    break;

                // TAX - transfer accumulator to X (NZ)
                case 0xaa:
                    X = A;
                    zf = ((X & 0xff) == 0x00);
                    nf = ((X & 0x80) == 0x80);
                    break;

                // TSX - transfer SP to X (NZ)
                case 0xba:
                    X = SP;
                    zf = ((X & 0xff) == 0x00);
                    nf = ((X & 0x80) == 0x80);
                    break;

                // TXA - transfer X to A (NZ)
                case 0x8a:
                    A = X;
                    zf = ((A & 0xff) == 0x00);
                    nf = ((A & 0x80) == 0x80);
                    break;

                // TXS - transfer X to SP (NZ)
                case 0x9a:
                    SP = X;
                    zf = ((SP & 0xff) == 0x00);
                    nf = ((SP & 0x80) == 0x80);
                    break;

                // TYA - transfer Y to A (NZ)
                case 0x98:
                    A = Y;
                    zf = ((A & 0xff) == 0x00);
                    nf = ((A & 0x80) == 0x80);
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

                // Absolute mode retrieves the byte at the indicated memory location
                case AddressModes.Absolute:             
                    oper = memory[ GetImmWord() ];
                    break;
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
                    oper = GetWordFromMemory((GetImmByte() + X) & 0xff);
                    break;

                // The Indirect Indexed works a bit differently than above.
                // The Y register is added *after* the deferencing instead of before.
                case AddressModes.IndirectY:

                    imm = GetImmByte();
                    if (_currentOP.CheckPageBoundary)
                    {
                        if (imm == 0xff) _extraCycles = 1;
                    }
                    oper = GetWordFromMemory(imm) + Y;
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
            return (ushort)((memory[PC - 1] << 8 | memory[PC - 2]) & 0xffff);
        }

        private byte GetImmByte()
        {
            return memory[PC - 1];
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
            memory[(0x0100 | SP)] = (byte)(data & 0xff);
            memory[(0x0100 | (SP-1))] = (byte)(data >> 8);
            SP -= 2;
        }

        private byte PopByte()
        {
            SP++;
            return memory[(0x0100 | SP)];
        }
        
        private ushort PopWord()
        {
            SP += 2;

            ushort idx = (ushort)(0x0100 | SP);
            return (ushort)((memory[idx - 1] << 8 | memory[idx]) & 0xffff);
        }
    }
}
