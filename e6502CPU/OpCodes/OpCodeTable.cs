using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace e6502CPU
{
    public class OpCodeTable
    {
        public OpCodeRecord[] OpCodes { get; private set; }

        public OpCodeTable()
        {
            OpCodes = new OpCodeRecord[0xff+1];
            for( int ii=0; ii<=0xff; ii++)
            {
                OpCodes[ii] = new OpCodeRecord();
            }
            CreateTable();
        }

        private void CreateTable()
        {
            StreamReader sr = new StreamReader("OpcodeList.txt");
            string line;

            string address;
            string instruction;
            string opcode;
            string bytes;
            string cycles;

            int rec_opcode;
            string rec_instr;
            AddressModes rec_mode;
            int rec_bytes;
            int rec_cycles;

            // throw away the first two lines
            sr.ReadLine();
            sr.ReadLine();
            do
            {
                line = sr.ReadLine();

                if (line.Length > 40)
                {
                    address = line.Substring(0, 14).Trim();
                    instruction = line.Substring(14, 14).Trim();
                    opcode = line.Substring(28, 6).Trim();
                    bytes = line.Substring(34, 6).Trim();
                    cycles = line.Substring(40,1).Trim();
                    

                    if( !int.TryParse(opcode, System.Globalization.NumberStyles.AllowHexSpecifier, null, out rec_opcode))
                    {
                        throw new InvalidDataException("Line + [" + line + "] (opc) has invalid data");
                    }

                    try
                    {
                        rec_instr = instruction.Substring(0, 3);
                    }
                    catch
                    {
                        throw new InvalidDataException("Line + [" + line + "] (assembler) has invalid data");
                    }

                    switch(address)
                    {
                        case "accumulator":
                            rec_mode = AddressModes.Accumulator;
                            break;
                        case "absolute":
                            rec_mode = AddressModes.Absolute;
                            break;
                        case "absolute,X":
                            rec_mode = AddressModes.AbsoluteX;
                            break;
                        case "absolute,Y":
                            rec_mode = AddressModes.AbsoluteY;
                            break;
                        case "immediate":
                            rec_mode = AddressModes.Immediate;
                            break;
                        case "implied":
                            rec_mode = AddressModes.Implied;
                            break;
                        case "indirect":
                            rec_mode = AddressModes.Indirect;
                            break;
                        case "(indirect,X)":
                            rec_mode = AddressModes.XIndirect;
                            break;
                        case "(indirect),Y":
                            rec_mode = AddressModes.IndirectY;
                            break;
                        case "relative":
                            rec_mode = AddressModes.Relative;
                            break;
                        case "zeropage":
                            rec_mode = AddressModes.ZeroPage;
                            break;
                        case "zeropage,X":
                            rec_mode = AddressModes.ZeroPageX;
                            break;
                        case "zeropage,Y":
                            rec_mode = AddressModes.ZeroPageY;
                            break;
                        default:
                            throw new InvalidDataException("Line + [" + line + "] (addressing) has invalid data");

                    }
                    if (!int.TryParse(bytes, out rec_bytes))
                    {
                        throw new InvalidDataException("Line + [" + line + "] (bytes) has invalid data");
                    }
                    if (!int.TryParse(cycles, out rec_cycles))
                    {
                        throw new InvalidDataException("Line + [" + line + "] (cycles) has invalid data");
                    }

                    OpCodes[rec_opcode] = new OpCodeRecord(rec_opcode, rec_instr, rec_mode, rec_bytes, rec_cycles);

                }

            } while (!sr.EndOfStream);

            line = "Success";

        }

    }
}
