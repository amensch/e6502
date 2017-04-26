using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Untari.CPU;

namespace UntariTests
{
    [TestClass]
    public class e6502TestTSB
    {
        [TestMethod]
        public void TestTSB1()
        {
            TestBus bus = new TestBus();
            e6502 cpu = new e6502(e6502Type.CMOS, bus);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0xa6,      // LDA #$A6
                                               0x85, 0x00,      // STA $00
                                               0xa9, 0x33,      // LDA #$33
                                               0x04, 0x00 });   // TSB $00

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0xb7, bus.GetByte(0x0000), "TSB failed");
            Assert.AreEqual(0x33, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.ZF, "ZF failed");               
        }

        [TestMethod]
        public void TestTSB2()
        {
            TestBus bus = new TestBus();
            e6502 cpu = new e6502(e6502Type.CMOS, bus);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0xa6,      // LDA #$A6
                                               0x85, 0x00,      // STA $00
                                               0xa9, 0x41,      // LDA #$41
                                               0x04, 0x00 });   // TSB $00

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0xe7, bus.GetByte(0x0000), "TSB failed"); 
            Assert.AreEqual(0x41, cpu.A, "A failed");
            Assert.AreEqual(true, cpu.ZF, "ZF failed");             
        }
    }
}
