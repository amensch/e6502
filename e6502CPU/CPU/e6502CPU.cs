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
        private int A;
        // Index Registers
        private int X;
        private int Y;
        // Stack Pointer
        private int SP;
        // Progream Counter
        private int PC;

        // Status Registers (in order bit 7 to 0)
        private bool sf;    // sign flag (N)
        private bool of;    // overflow flag (V)
        // bit 5 not used
        private bool bf;    // breakpoint flag (B)
        private bool bcd;   // binary coded decimal flag (D)
        private bool intf;  // interrupt flag (I)
        private bool zf;    // zero flag (Z)
        private bool cf;    // carry flag (C)

        // RAM
        private byte[] memory;

        // List of op codes and their attributes
        private OpCodeTable _opCodeTable;

        // The current opcode
        private OpCodeRecord _currentOP;

        // The current operand (for debugging)
        private int _operand;

        public e6502CPU()
        {
            memory = new byte[0x10000];
            _opCodeTable = new OpCodeTable();
        }

        public void LoadProgram(int startingAddress, byte[] program)
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

            ExecuteInstruction();

            PC += _currentOP.Bytes;
            return _currentOP.Cycles;
        }

        private void ExecuteInstruction()
        {
            // Get operand (if applicable)
            _operand = GetOperand(_currentOP.AddressMode);

            switch(_currentOP.OpCode)
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
                    A += memory[_operand];
                    if (cf) A++;
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
                default:
                    break;
            }
        }

        private int GetOperand(AddressModes mode)
        {
            int oper = 0;
            switch (mode)
            {
                case AddressModes.Accumulator:
                    oper = A;
                    break;
                case AddressModes.Absolute:
                    oper = GetImmWord();
                    break;
                case AddressModes.AbsoluteX:
                    oper = GetImmWord() + X;
                    break;
                case AddressModes.AbsoluteY:
                    oper = GetImmWord() + Y;
                    break;
                case AddressModes.Immediate:
                    oper = GetImmByte();
                    break;
                case AddressModes.Implied:
                    break;
                case AddressModes.Indirect:
                    oper = GetWordFromMemory(GetImmWord());
                    break;
                case AddressModes.XIndirect:
                    oper = GetWordFromMemory((GetImmByte() + X) & 0xff);
                    break;
                case AddressModes.IndirectY:
                    oper = GetWordFromMemory(GetImmByte()) + Y;
                    break;
                case AddressModes.Relative:
                    oper = PC + SignExtend(GetImmByte());
                    break;
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

        private int GetWordFromMemory(int address)
        {
            return (memory[address + 1] << 8 | memory[address]) & 0xffff;
        }

        private int GetImmWord()
        {
            return (memory[PC + 2] << 8 | memory[PC + 1]) & 0xffff;
        }

        private int GetImmByte()
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
    }
}
