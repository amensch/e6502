using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Untari.CPU;

namespace UntariTests
{
    [TestClass]
    public class e6502TestTRB
    {
        [TestMethod]
        public void TestTRB1()
        {
            TestRAM ram = new TestRAM();
            e6502 cpu = new e6502(e6502Type.CMOS, ram);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0xa6,      // LDA #$A6
                                               0x85, 0x00,      // STA $00
                                               0xa9, 0x33,      // LDA #$33
                                               0x14, 0x00 });   // TRB $00

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x84, ram.GetByte(0x0000), "TRB failed");    // ($A6 AND ($33 XOR $FF))
            Assert.AreEqual(0x33, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.ZF, "ZF failed");                // ($A6 AND $33) = $22
        }

        [TestMethod]
        public void TestTRB2()
        {
            TestRAM ram = new TestRAM();
            e6502 cpu = new e6502(e6502Type.CMOS, ram);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0xa6,      // LDA #$A6
                                               0x85, 0x00,      // STA $00
                                               0xa9, 0x41,      // LDA #$41
                                               0x14, 0x00 });   // TRB $00

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0xa6, ram.GetByte(0x0000), "TRB failed");    // ($A6 AND ($41 XOR $FF))
            Assert.AreEqual(0x41, cpu.A, "A failed");
            Assert.AreEqual(true, cpu.ZF, "ZF failed");                // ($A6 AND $41) = $00
        }
    }
}
