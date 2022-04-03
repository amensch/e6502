namespace KDS.e6502
{
    public interface IBusDevice
    {
        byte Read(ushort address);
        void Write(ushort address, byte data);
    }
}
