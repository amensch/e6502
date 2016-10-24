using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e6502CPU
{
    public class OpCodeRecord
    {
        public int OpCode { get; private set; }
        public string Instruction { get; private set; }
        public AddressModes AddressMode { get; private set; }
        public int Bytes { get; private set; }
        public int Cycles { get; private set; }
        public bool IsValid { get; private set; }

        public OpCodeRecord()
        {
            Bytes = 1;
            IsValid = false;
        }

        public OpCodeRecord(int opcode, string instruction, AddressModes addressmode, int bytes, int cycles)
        {
            OpCode = opcode;
            Instruction = instruction;
            AddressMode = addressmode;
            Bytes = bytes;
            Cycles = cycles;
            IsValid = true;
        }

        public string Dasm()
        {
            if (!IsValid)
                return "???";

            if ( AddressMode == AddressModes.Accumulator)
            {
                return Instruction + " A";
            }
            return "???";
        }

        public string Dasm(int oper)
        {
            if (!IsValid)
                return "???";

            string dasm = Instruction;
            switch (AddressMode)
            {
                case AddressModes.Accumulator:
                    dasm += " A";
                    break;
                case AddressModes.Absolute:
                    dasm += " $" + oper.ToString("X4");
                    break;
                case AddressModes.AbsoluteX:
                    dasm += " $" + oper.ToString("X4") + ",X";
                    break;
                case AddressModes.AbsoluteY:
                    dasm += " $" + oper.ToString("X4") + ",Y";
                    break;
                case AddressModes.Relative:
                    dasm += " $" + oper.ToString("X2");
                    break;
                case AddressModes.ZeroPage:
                    dasm += " $" + oper.ToString("X2");
                    break;
                case AddressModes.ZeroPageX:
                    dasm += " $" + oper.ToString("X2") + ",X";
                    break;
                case AddressModes.ZeroPageY:
                    dasm += " $" + oper.ToString("X2") + ",Y";
                    break;
                case AddressModes.Immediate:
                    dasm += " #$" + oper.ToString("X2");
                    break;
                case AddressModes.Indirect:
                    dasm += " ($" + oper.ToString("X4") + ")";
                    break;
                case AddressModes.XIndirect:
                    dasm += " ($" + oper.ToString("X2") + ",X)";
                    break;
                case AddressModes.IndirectY:
                    dasm += " ($" + oper.ToString("X2") + "),Y";
                    break;
                case AddressModes.Implied: // do nothing
                default:
                    break;
            }
            return dasm;
        }

        // this is so something useful is displayed in the watch window
        public override string ToString()
        {
            if (!IsValid)
                return "???";
            else
                return string.Format("{0} {1} {2}", OpCode.ToString("X2"), Instruction, AddressMode.ToString());
        }
    }
}
