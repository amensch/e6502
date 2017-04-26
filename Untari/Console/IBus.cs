using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Untari.Console
{
    public interface IBus
    {
        byte GetByte(int address);
        void WriteByte(int address, byte data);
        void LoadProgram(ushort startingAddress, byte[] program);
    }
}
