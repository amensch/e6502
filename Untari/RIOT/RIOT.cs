using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Untari.RIOT
{
    public class RIOT : IBusDevice
    {
        // Chip Select Mask (A7=1, A12=0)
        private const int CHIP_SELECT_MASK = 0x1280;
        private const int CHIP_SELECT = 0x0280;
        private const int CHIP_RAM_SELECT = 0x0080;

        public byte GetByte(int address)
        {
            if( (address & CHIP_SELECT_MASK) == CHIP_SELECT)
            {

            }
            else if((address & CHIP_SELECT_MASK) == CHIP_SELECT)
            {

            }
            else if((address & CHIP_SELECT_MASK) == CHIP_SELECT)
            {

            }
        }

        public void WriteByte(int address, byte data)
        {
            
        }
    }
}
