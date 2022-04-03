namespace KDS.e6502.Utility
{
    internal class CPUMath
    {
        public static byte BCDToHex(int result)
        {
            if (result > 0xff)
                throw new InvalidOperationException("Invalid BCD to hex number: " + result.ToString());

            if (result <= 9)
                return (byte)result;
            else
                return (byte)(((result / 10) << 4) + (result % 10));
        }

        public static int HexToBCD(byte oper)
        {
            // validate input is valid packed BCD 
            if (oper > 0x99)
                throw new InvalidOperationException("Invalid BCD number: " + oper.ToString("X2"));
            if ((oper & 0x0f) > 0x09)
                throw new InvalidOperationException("Invalid BCD number: " + oper.ToString("X2"));

            return ((oper >> 4) * 10) + (oper & 0x0f);
        }
        public static int SignExtend(int num)
        {
            if (num < 0x80)
                return num;
            else
                return (0xff << 8 | num) & 0xffff;
        }
    }
}
