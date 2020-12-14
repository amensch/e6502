/*
 * e6502: A complete 6502 CPU emulator.
 * Copyright 2016 Adam Mensch
 */

using System;
using System.IO;

namespace KDS.e6502CPU
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
            OpCodeReader oplist = new OpCodeReader();

            string address;
            string instruction;
            string opcode;
            string bytes;
            string cycles;

            int rec_opcode;
            string rec_instr;
            AddressModes rec_mode;
            ushort rec_bytes;
            int rec_cycles;
            bool rec_checkPageBoundary;
            bool rec_checkBranchPage;

            foreach( String line in oplist )
            {
                if (line.Length > 40)
                {
                    address = line.Substring(0, 14).Trim();
                    instruction = line.Substring(14, 14).Trim();
                    opcode = line.Substring(28, 6).Trim();
                    bytes = line.Substring(34, 6).Trim();
                    cycles = line.Substring(40).Trim();

                    rec_checkPageBoundary = (cycles.Length == 2);
                    rec_checkBranchPage = (cycles.Length == 3);
                    cycles = cycles.Substring(0,1);
                    

                    if( !int.TryParse(opcode, System.Globalization.NumberStyles.AllowHexSpecifier, null, out rec_opcode))
                    {
                        throw new InvalidDataException("Line + [" + line + "] (opc) has invalid data");
                    }

                    try
                    {
                        int idx = instruction.IndexOf(" ");
                        if (idx > -1)
                            rec_instr = instruction.Substring(0, instruction.IndexOf(" "));
                        else
                            rec_instr = instruction.TrimEnd();
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
                        case "zeropage,0":
                            rec_mode = AddressModes.ZeroPage0;
                            break;
                        case "branchext":
                            rec_mode = AddressModes.BranchExt;
                            break;
                        default:
                            throw new InvalidDataException("Line + [" + line + "] (addressing) has invalid data");

                    }
                    if (!ushort.TryParse(bytes, out rec_bytes))
                    {
                        throw new InvalidDataException("Line + [" + line + "] (bytes) has invalid data");
                    }
                    if (!int.TryParse(cycles, out rec_cycles))
                    {
                        throw new InvalidDataException("Line + [" + line + "] (cycles) has invalid data");
                    }

                    OpCodes[rec_opcode] = new OpCodeRecord((byte)rec_opcode, rec_instr, rec_mode, rec_bytes, rec_cycles,
                        rec_checkPageBoundary, rec_checkBranchPage);

                }

            } 

        }

    }
}
