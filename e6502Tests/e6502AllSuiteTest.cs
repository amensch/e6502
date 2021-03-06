﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using KDS.e6502CPU;
using System.IO;
using System.Diagnostics;

namespace KDS.e6502Tests
{
    [TestClass]
    public class e6502AllSuiteTest
    {
        [TestMethod]
        public void RunAllSuiteTest()
        {
            /*
             *  Load and run test program found here:
             *  https://codegolf.stackexchange.com/questions/12844/emulate-a-mos-6502-cpu?rq=1
             *  If the program gets to PC=$45C0 then all tests passed.
             */

            var bus = new BusDevice(0x10000, File.ReadAllBytes(@"..\..\Resources\AllSuiteA.bin"), 0x4000);
            e6502 cpu = new e6502( bus, e6502Type.CMOS );
            cpu.Boot(0x0400);
            //cpu.LoadProgram( 0x4000, File.ReadAllBytes( @"..\..\Resources\AllSuiteA.bin" ) );
            //cpu.PC = 0x0400;

            ushort prev_pc;
            long instr_count = 0;
            do
            {
                instr_count++;
                prev_pc = cpu.PC;
                cpu.ExecuteNext();
            } while( prev_pc != cpu.PC );

            Debug.WriteLine( "Instructions: " + instr_count.ToString( "N0" ) );

            Assert.AreEqual( 0x45c0, cpu.PC, "Test program failed at $" + cpu.PC.ToString( "X4" ) );
            Assert.AreEqual( 0xff, cpu.SystemBus.Read( 0x0210 ), "Test value failed" );    
        }

        [TestMethod]
        public void RunAllSuiteTestByTick()
        {
            /*
             *  Load and run test program found here:
             *  https://codegolf.stackexchange.com/questions/12844/emulate-a-mos-6502-cpu?rq=1
             *  If the program gets to PC=$45C0 then all tests passed.
             */

            var bus = new BusDevice(0x10000, File.ReadAllBytes(@"..\..\Resources\AllSuiteA.bin"), 0x4000);
            e6502 cpu = new e6502(bus, e6502Type.CMOS);
            cpu.Boot(0x0400);
            //cpu.LoadProgram( 0x4000, File.ReadAllBytes( @"..\..\Resources\AllSuiteA.bin" ) );
            //cpu.PC = 0x0400;

            ushort prev_pc;
            long instr_count = 0;
            long cycle_count = 0;
            do
            {
                instr_count++;
                prev_pc = cpu.PC;
                cycle_count += cpu.ClocksForNext();
                cpu.ExecuteNext();

            } while (prev_pc != cpu.PC);

            Debug.WriteLine("Cycles: " + cycle_count.ToString("N0"));
            Debug.WriteLine("Instructions: " + instr_count.ToString("N0"));

            Assert.AreEqual(0x45c0, cpu.PC, "Test program failed at $" + cpu.PC.ToString("X4"));
            Assert.AreEqual(0xff, cpu.SystemBus.Read(0x0210), "Test value failed");
        }
    }
}
