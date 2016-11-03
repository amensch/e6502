using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e6502CPU
{
    /*
     *  The addressing mode indicates the source data in an instruction.
     *  There are two types of addressing modes, indexed and non-indexed.
     */

    public enum AddressModes
    {
        // Non-indexed, non memory
        Accumulator,    // operand is A accumulator

        Absolute,       // operand is 16 bit immediate
        AbsoluteX,      // operand is 16 bit immediate + X + carry
        AbsoluteY,      // operand is 16 bit immediate + Y + carry

        Immediate,      // operand is 8 bit immediate

        Implied,        // operand is implied

        Indirect,       // 16 bit immediate effective address
        XIndirect,      // effective zeropage address, 8 bit immediate incremented by X no carry
        IndirectY,      // effective address incremented by Y with carry

        Relative,       // branch target is PC + 8bit offset (signed)

        ZeroPage,       // operand is address &00xx (xx 8 bit immediate)
        ZeroPage0,      // operand is address &00xx (works like ZeroPageY with Y=0)
        ZeroPageX,      // operand is address &00xx (xx 8 bit immediate) incremented by X
        ZeroPageY,      // operand is address &00xx (xx 8 bit immediate) incremented by Y

        BranchExt       // extended 65C02 instructions - 3 byte branch instructions

    };
}
