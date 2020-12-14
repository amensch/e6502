using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KDS.e6502CPU;

namespace KDS.e6502Tests
{
    [TestClass]
    public class e6502TestBBS
    {
        [TestMethod]
        public void TestBBS0()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0x55,            // LDA #$55
                                               0x85, 0x00,            // STA $00
                                               0x8f, 0x00, 0x11 });   // BBS0 $00, $11

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x18, cpu.PC, "BBS0 failed");
        }

        [TestMethod]
        public void TestBBS1()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0x55,            // LDA #$55
                                               0x85, 0x00,            // STA $00
                                               0x9f, 0x00, 0x11 });   // BBS1 $00, $11

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x07, cpu.PC, "BBS1 failed");
        }

        [TestMethod]
        public void TestBBS2()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0x55,            // LDA #$55
                                               0x85, 0x00,            // STA $00
                                               0xaf, 0x00, 0x11 });   // BBS2 $00, $11

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x18, cpu.PC, "BBS2 failed");
        }

        [TestMethod]
        public void TestBBS3()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0x55,            // LDA #$55
                                               0x85, 0x00,            // STA $00
                                               0xbf, 0x00, 0x11 });   // BBS3 $00, $11

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x07, cpu.PC, "BBS3 failed");
        }

        [TestMethod]
        public void TestBBS4()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0x55,            // LDA #$55
                                               0x85, 0x00,            // STA $00
                                               0xcf, 0x00, 0x11 });   // BBS4 $00, $11

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x18, cpu.PC, "BBS4 failed");
        }

        [TestMethod]
        public void TestBBS5()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0x55,            // LDA #$55
                                               0xd5, 0x00,            // STA $00
                                               0xcf, 0x00, 0x11 });   // BBS5 $00, $11

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x07, cpu.PC, "BBS5 failed");
        }

        [TestMethod]
        public void TestBBS6()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0x55,            // LDA #$55
                                               0x85, 0x00,            // STA $00
                                               0xef, 0x00, 0x11 });   // BBS6 $00, $11

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x18, cpu.PC, "BBS6 failed");
        }

        [TestMethod]
        public void TestBBS7()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] { 0xa9, 0x55,            // LDA #$55
                                               0x85, 0x00,            // STA $00
                                               0xff, 0x00, 0x11 });   // BBS7 $00, $11

            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x07, cpu.PC, "BBS7 failed");
        }

    }
}
