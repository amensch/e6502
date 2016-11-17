using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using e6502CPU;
using System.IO;
using System.Diagnostics;

namespace e6502Tests
{
    [TestClass]
    public class e6502InterruptTest
    {
        [TestMethod]
        public void RunInterruptTest()
        {
            /*
             *  This loads a test program that tests interrupt handling in the 65C02.
             *  If the program gets to PC=$06ec then all tests passed.
             */

            e6502 cpu = new e6502(e6502Type.NMOS);
            cpu.LoadProgram(0x0400, File.ReadAllBytes(@"..\..\Resources\6502_interrupt_test.bin"));
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

                // Add interrupts where expected in the test.
                switch (prev_pc)
                {
                    // IRQ tests
                    case 0x0434:
                    case 0x0464:
                    case 0x04a3:
                    case 0x04de:
                        cpu.IRQWaiting = true;
                        break;

                    // NMI tests
                    case 0x05c8:
                    case 0x05f8:
                    case 0x0637:
                    case 0x0672:
                        cpu.NMIWaiting = true;
                        break;

                    // IRQ and NMI waiting tests
                    case 0x06a0:
                    case 0x06db:
                        cpu.IRQWaiting = true;
                        cpu.NMIWaiting = true;
                        break;
                }

            } while (prev_pc != cpu.PC);
            sw.Stop();

            Debug.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString() + " ms");
            Debug.WriteLine("Cycles: " + cycle_count.ToString("N0"));
            Debug.WriteLine("Instructions: " + instr_count.ToString("N0"));

            Assert.AreEqual(0x06ec, cpu.PC, "Test program failed at $" + cpu.PC.ToString("X4"));
        }

    }
}
