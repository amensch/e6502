using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using e6502CPU;

namespace Untari.TIA
{
    public class TIA
    {
        /*
         * TIA Registers: $0000-007F
         * 
         * General TIA Timing:
         * 1 CPU clock is 3 TIA clocks
         * 
         * Screen Drawing:
         *      3 lines of vertical sync
         *      37 lines of vertical blank
         *      192 lines of actual picture (68 clocks horizontal blank followed by 160 clocks of drawing)
         *      30 lines of vertical overscan
         * 
         * Vertical Timing:
         *  When 262 lines have been scanned the TV must be signaled to blank the beam and return to the top.
         *  
         *  The TIA must transmit vertical sync for 3 scanlines by writing a "1" in D1 of VSYNC register,
         *  count 3 lines, then write a "0" in D1 of VSYNC.
         *  
         *  To turn off the beam a "1" in D1 of VBLANK is set, then 37 lines are counted and then 
         *  a "0" is written in D1 of VBLANK.
         *
         * The drawing area consists of a static playfield, 2 players, 2 missiles, and 1 ball.
         * The TIA is responsible for collision detection between the movable objects.
         */


        private const int HBLANK = 68;
        private const int VDRAWMIN = 41;
        private const int VDRAWMAX = 232;

        private const int HMAX = 228;
        private const int VMAX = 262;

        private e6502 cpu;

        private int h_line;
        private int v_line;

        public TIA(e6502 _cpu)
        {
            cpu = _cpu;
            h_line = 0;
            v_line = 0;
        }

        /// <summary>
        /// Processes one tick of the TIA clock
        /// </summary>
        public void Tick()
        {
            // if WSYNC has a value clear the RDY signal in the CPU
            if (cpu.memory[TIAAddress.WSYNC] > 0)
            {
                cpu.RDY = false;

                // this doesn't happen in reality
                cpu.memory[TIAAddress.WSYNC] = 0;
            }

            h_line++;
            if (h_line > HMAX)
            {

                h_line = 1;
                v_line++;
                
                // new line, re-enable the RDY signal
                cpu.RDY = true;

                // New screen starting
                if (v_line > VMAX)
                {
                    v_line = 1;
                }
            }
            Draw();
        }

        /// <summary>
        /// Used to "catch up" the TIA to the CPU.
        /// </summary>
        /// <param name="cpuclocks">Number of CPU cycles processed</param>
        public void Tick(int cpuclocks)
        {
            int ticks = cpuclocks * 3;

            for( int ii = 1; ii <= ticks; ii++ )
            {
                Tick();
            }

        }

        private void Draw()
        {
            // check for transition of VSYNC
            // check for transition of VBLANK
            // check for use of WSYNC

            // this is the actual drawing area of the screen
            if( ( h_line > HBLANK ) && ( v_line > VDRAWMIN ) && ( v_line < VDRAWMAX ) )
            {

            }
        }

        private void SetBit(int bitnum, int address)
        {
            int setbit = 0x01;
            for (int ii = 0; ii < 8; ii++)
            {
                setbit = setbit << 1;
            }
            cpu.memory[address] = (byte)(cpu.memory[address] | setbit);
        }

        private void ClearBit(int address, int bitnum)
        {
            int clearbit = 0x01;
            for (int ii = 0; ii < 8; ii++)
            {
                clearbit = clearbit << 1;
            }
            cpu.memory[address] = (byte)(cpu.memory[address] & ~clearbit);
        }

    }
}
