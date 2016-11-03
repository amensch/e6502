using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using e6502CPU;
using System.IO;
using System.Diagnostics;

namespace e6502Tests
{
    [TestClass]
    public class e6502FuncTest
    {
        [TestMethod]
        public void RunFuncTestProgram()
        {
            /*
             *  This loads a test program that exercises all the standard instructions of the 6502.
             *  If the program gets to PC=$3399 then all tests passed.
             */

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
            long instr_count = 0;
            long cycle_count = 0;
            Stopwatch sw = new Stopwatch();

            sw.Start();
            do
            {
                instr_count++;
                prev_pc = cpu.PC;
                cycle_count += cpu.ExecuteNext();
            } while (prev_pc != cpu.PC);
            sw.Stop();

            Debug.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString() + " ms");
            Debug.WriteLine("Cycles: " + cycle_count.ToString("N0"));
            Debug.WriteLine("Instructions: " + instr_count.ToString("N0"));

            double mhz = (cycle_count / sw.ElapsedMilliseconds) / 1000;
            Debug.WriteLine("Effective Mhz: " + mhz.ToString("N1"));

            Assert.AreEqual(0x3399, cpu.PC, "Test program failed at " + cpu.PC.ToString("X4"));
        }
    }
}
