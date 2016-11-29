using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Untari.TIA
{
    public class TIA
    {
        private const int HBLANK = 68;
        private const int VDRAWMIN = 41;
        private const int VDRAWMAX = 232;


        private int h_line;
        private int v_line;

        public TIA()
        {
            // initial state?
            h_line = 0;
            v_line = 0;
        }

        public void Tick(int cpuclocks)
        {
            // 3 TIA clocks for every 6502 clock.
            // Update state machine and make any other necessary updates.
            int new_hline = h_
        }


    }
}
