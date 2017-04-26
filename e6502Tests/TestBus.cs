using System;
using Untari.Console;

namespace e6502Tests
{
    public class TestBus : IBus
    {
        private const int MAX_MEMORY_SIZE = 0x10000;
        private byte[] _memory = new byte[MAX_MEMORY_SIZE];

        public byte GetByte(int address)
        {
            if (address >= MAX_MEMORY_SIZE)
                throw new ArgumentOutOfRangeException("address");

            return _memory[address];
        }

        public void WriteByte(int address, byte data)
        {
            if (address >= MAX_MEMORY_SIZE)
                throw new ArgumentOutOfRangeException("address");

            _memory[address] = data;
        }

        public void LoadProgram(ushort startingAddress, byte[] program)
        {
            program.CopyTo(_memory, startingAddress);
        }
    }
}
