using KDS.e6502CPU;
public class TestROM : IBusDevice
{
    private byte[] ROM;

    public TestROM(int size)
    {
        ROM = new byte[size];
    }

    public TestROM(int size, ushort loadAddress, byte[] data) : this(size)
    {
        Load(loadAddress, data);
    }

    public void Load(ushort loadAddress, byte[] data)
    {
        data.CopyTo(ROM, loadAddress);
    }

    public byte Read(ushort address)
    {
        return ROM[address];
    }

    public void Write(ushort address, byte data)
    {
        ROM[address] = data;
    }

}

