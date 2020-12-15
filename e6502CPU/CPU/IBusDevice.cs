namespace KDS.e6502CPU
{
    public interface IBusDevice
    {
        byte Read(int address);
        void Write(int address, byte data);
        ushort ReadWord(int address);
    }
}
