using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using e6502CPU;
using System.IO;

namespace e6502Tests
{
    [TestClass]
    public class e6502FuncTest
    {
        [TestMethod]
        public void RunFuncTestProgram()
        {
            e6502 cpu = new e6502();
            byte[] program;

            if (System.Environment.MachineName.StartsWith("US"))
            {
                program = File.ReadAllBytes(@"C:\Users\menschas\Source\6502_65C02_functional_tests\bin_files\6502_functional_test.bin");
            }
            else
            {
                program = File.ReadAllBytes(@"C:\Users\adam\Documents\My Projects\6502_65C02_functional_tests\bin_files\6502_functional_test.bin");
            }
            cpu.LoadProgram(0x0000, program);
            cpu.PC = 0x0400;

            ushort prev_pc;
            do
            {
                prev_pc = cpu.PC;
                cpu.ExecuteNext();
            } while (prev_pc != cpu.PC);

            Assert.AreEqual(0x3399, cpu.PC, "Test program failed at " + cpu.PC.ToString("X4"));
        }
    }
}
