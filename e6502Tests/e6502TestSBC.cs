using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using e6502CPU;

namespace e6502Tests
{
    [TestClass]
    public class e6502TestSBC
    {
        [TestMethod]
        public void TestSBC1()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] {  0x38,           // SEC
                                                0xa9, 0x00,     // LDA #$00
                                                0xe9, 0x01 });  // SBC #$01
                                                                // 0 - 1 = -1
                                                                // A=ff, V=0
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0xff, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.ZF, "ZF failed");
            Assert.AreEqual(true, cpu.NF, "NF failed");
            Assert.AreEqual(false, cpu.CF, "CF failed");
            Assert.AreEqual(false, cpu.VF, "VF failed");
        }

        [TestMethod]
        public void TestSBC2()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] {  0x38,           // SEC
                                                0xa9, 0x80,     // LDA #$80
                                                0xe9, 0x01 });  // SBC #$01
                                                                // -128 - 1 = -129
                                                                // A=7f, V=1
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x7f, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.ZF, "ZF failed");
            Assert.AreEqual(false, cpu.NF, "NF failed");
            Assert.AreEqual(true, cpu.CF, "CF failed");
            Assert.AreEqual(true, cpu.VF, "VF failed");
        }

        [TestMethod]
        public void TestSBC3()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] {  0x38,           // SEC
                                                0xa9, 0x7f,     // LDA #$7F
                                                0xe9, 0xff });  // SBC #$ff
                                                                // 127 - -1 = 128
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x80, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.ZF, "ZF failed");
            Assert.AreEqual(true, cpu.NF, "NF failed");
            Assert.AreEqual(false, cpu.CF, "CF failed");
            Assert.AreEqual(true, cpu.VF, "VF failed");
        }

        [TestMethod]
        public void TestSBC4()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] {  0x18,           // CLC
                                                0xa9, 0xc0,     // LDA #$C0
                                                0xe9, 0x40 });  // SBC #$40
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x7f, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.ZF, "ZF failed");
            Assert.AreEqual(false, cpu.NF, "NF failed");
            Assert.AreEqual(true, cpu.CF, "CF failed");
            Assert.AreEqual(true, cpu.VF, "VF failed");
        }


        [TestMethod]
        public void TestSBC5()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] {  0xf8,           // SED
                                                0x38,           // SEC
                                                0xa9, 0x46,     // LDA #$46
                                                0xe9, 0x12 });  // SBC #$12
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x34, cpu.A, "A failed");
            Assert.AreEqual(true, cpu.CF, "CF failed");
            //Assert.AreEqual(false, cpu.VF, "VF failed");
        }

        [TestMethod]
        public void TestSBC6()
        {
            e6502 cpu = new e6502(e6502Type.CMOS);
            cpu.LoadProgram(0x00, new byte[] {  0xf8,           // SED
                                                0x38,           // SEC
                                                0xa9, 0x12,     // LDA #$12
                                                0xe9, 0x21 });  // SBC #$21
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x91, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.CF, "CF failed");
        }
    }
}
