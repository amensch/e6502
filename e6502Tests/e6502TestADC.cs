using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using e6502CPU;

namespace e6502Tests
{
    [TestClass]
    public class e6502TestADC
    {
        [TestMethod]
        public void TestADC1()
        {
            e6502 cpu = new e6502();
            cpu.LoadProgram(0x00, new byte[] {  0x18,           // CLC
                                                0xa9, 0x01,     // LDA #$01
                                                0x69, 0x01 });  // ADC #$01
                                                // 1 + 1 = 2
                                                // A=2, V=0
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x02, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.ZF, "ZF failed");
            Assert.AreEqual(false, cpu.NF, "NF failed");
            Assert.AreEqual(false, cpu.CF, "CF failed");
            Assert.AreEqual(false, cpu.VF, "VF failed");
        }

        [TestMethod]
        public void TestADC2()
        {
            e6502 cpu = new e6502();
            cpu.LoadProgram(0x00, new byte[] {  0x18,           // CLC
                                                0xa9, 0x01,     // LDA #$01
                                                0x69, 0xff });  // ADC #$ff
                                                // 1 + -1 = 0
                                                // A=0, V=0
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x00, cpu.A, "A failed");
            Assert.AreEqual(true, cpu.ZF, "ZF failed");
            Assert.AreEqual(false, cpu.NF, "NF failed");
            Assert.AreEqual(true, cpu.CF, "CF failed");
            Assert.AreEqual(false, cpu.VF, "VF failed");
        }

        [TestMethod]
        public void TestADC3()
        {
            e6502 cpu = new e6502();
            cpu.LoadProgram(0x00, new byte[] {  0x18,           // CLC
                                                0xa9, 0x7f,     // LDA #$7f
                                                0x69, 0x01 });  // ADC #$01
                                                                // 127 + 1 = 128
                                                                // A=0, V=1
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
        public void TestADC4()
        {
            e6502 cpu = new e6502();
            cpu.LoadProgram(0x00, new byte[] {  0x18,           // CLC
                                                0xa9, 0x80,     // LDA #$80
                                                0x69, 0xff });  // ADC #$ff
                                                                // -128 + -1 = -129
                                                                //  A=-129 V=1
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
        public void TestADC5()
        {
            e6502 cpu = new e6502();
            cpu.LoadProgram(0x00, new byte[] {  0x38,           // SEC
                                                0xa9, 0x3f,     // LDA #$3F
                                                0x69, 0x40 });  // ADC #$40
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
        public void TestADC_BCD1()
        {
            e6502 cpu = new e6502();
            cpu.LoadProgram(0x00, new byte[] {  0xf8,           // SED
                                                0xa9, 0x58,     // LDA #$58
                                                0x69, 0x46 });  // ADC #$46
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x04, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.ZF, "ZF failed");
            Assert.AreEqual(false, cpu.NF, "NF failed");
            Assert.AreEqual(true, cpu.CF, "CF failed");
            //Assert.AreEqual(true, cpu.VF, "VF failed");
        }

        [TestMethod]
        public void TestADC_BCD2()
        {
            e6502 cpu = new e6502();
            cpu.LoadProgram(0x00, new byte[] {  0xf8,           // SED
                                                0x38,           // SEC
                                                0xa9, 0x58,     // LDA #$58
                                                0x69, 0x46 });  // ADC #$46
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x05, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.ZF, "ZF failed");
            Assert.AreEqual(false, cpu.NF, "NF failed");
            Assert.AreEqual(true, cpu.CF, "CF failed");
            //Assert.AreEqual(true, cpu.VF, "VF failed");
        }

        [TestMethod]
        public void TestADC_BCD3()
        {
            e6502 cpu = new e6502();
            cpu.LoadProgram(0x00, new byte[] {  0xf8,           // SED
                                                0xa9, 0x15,     // LDA #$15
                                                0x69, 0x26 });  // ADC #$26
            cpu.ExecuteNext();
            cpu.ExecuteNext();
            cpu.ExecuteNext();

            Assert.AreEqual(0x41, cpu.A, "A failed");
            Assert.AreEqual(false, cpu.CF, "CF failed");
        }
    }
}
