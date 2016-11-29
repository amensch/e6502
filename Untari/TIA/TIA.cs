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
         * Horizontal Timing:  68 TIA clocks hBlank, 160 TIA clocks drawing
         * Total of 228 horizontal scan lines
         * Vertical Timing: 3 lines vSync, 37 lines vBlank, 192 lines drawing, 30 lines overscan
         * Total of 262 vertical scan lines
         * 
         * WSYNC regsiter is used to halt the CPU and resyncronize at the end of a line
         * 
         * Vertical Timing:
         *  When 262 lines have been scanned the TV must be signaled to blank the beam and return to the top.
         *  The TIA must transmit vertical sync for 3 scanlines by writing a "1" in D1 of VSYNC register,
         *  count 3 lines, then write a "0" in D1 of VSYNC.
         *  
         *  To turn off the beam a "1" in D1 of VBLANK is set, then 37 lines are counted and then 
         *  a "0" is written in D1 of VBLANK.
         *
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

            // initial state?
            h_line = 0;
            v_line = 0;
        }

        public void Tick(int cpuclocks)
        {
            // 3 TIA clocks for every 6502 clock.
            // Update state machine and make any other necessary updates.
            int ticks = cpuclocks * 3;

            for( int ii = 1; ii <= ticks; ii++ )
            {
                h_line++;
                if( h_line > HMAX)
                {
                    h_line = 1;
                    v_line++;
                    if(v_line > VMAX)
                    {
                        v_line = 1;
                    }
                }
                Draw();
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


    }
}
