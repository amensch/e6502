namespace KDS.e6502
{
    /// <summary>
    /// Example Bus Device for loading a 
    /// </summary>
    public class BusDevice : IBusDevice
    {
        private readonly byte[] ram = new byte[0x10000];

        public BusDevice(byte[] program, ushort loadingAddress)
        {
            Load(program, loadingAddress);
        }

        public BusDevice(byte[] program) : this(program, 0) { }

        public void Load(byte[] program)
        {
            Load(program, 0);
        }

        public void Load(byte[] program, int loadingAddress)
        {
            program.CopyTo(ram, loadingAddress);
        }

        public virtual byte Read(ushort address)
        {
            return ram[address];
        }

        public virtual void Write(ushort address, byte data)
        {
            ram[address] = data;
        }
    }
}
