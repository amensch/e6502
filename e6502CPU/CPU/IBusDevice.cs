namespace KDS.e6502CPU
{
    public interface IBusDevice
    {
        byte Read(ushort address);
        void Write(ushort address, byte data);
        ushort ReadWord(ushort address);
    }
}
