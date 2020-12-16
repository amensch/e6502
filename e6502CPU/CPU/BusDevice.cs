namespace KDS.e6502CPU
{
    public class BusDevice : IBusDevice
    {
        private byte[] ram;
        private int MaxSize;

        public BusDevice(int maxSize, byte[] program, ushort loadingAddress) : this(maxSize)
        {
            Load(program, loadingAddress);
        }

        public BusDevice(int maxSize, byte[] program) : this(maxSize, program, 0) { }

        public BusDevice(int maxSize)
        {
            MaxSize = maxSize;
            ram = new byte[maxSize];
        }

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
