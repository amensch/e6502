using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Untari.CPU;
using System.IO;
using System.Diagnostics;

namespace UntariTests
{
    [TestClass]
    public class e6502ExtendedTest
    {
        [TestMethod]
        public void RunExtTestProgram()
        {
            /*
             *  This loads a test program that exercises all the extended instructions of the 65C02 (CMOS).
             *  If the program gets to PC=24a8 then all tests passed.
             */

            TestBus bus = new TestBus();
            e6502 cpu = new e6502(e6502Type.CMOS, bus);
            cpu.LoadProgram(0x0000, File.ReadAllBytes(@"..\..\Resources\65C02_extended_opcodes_test.bin"));
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

            Assert.AreEqual(0x24a8, cpu.PC, "Test program failed at $" + cpu.PC.ToString("X4"));
        }
    }
}
