using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Untari.CPU;

namespace Untari.PIA
{
    public class PIA
    {
        /*
         * The PIA is a 6532 peripheral interface adapter which has three functions:
         *      1. a programmable timer
         *      2. 128 bytes of RAM
         *      3. two 8 bit I/O ports
         *      
         * The PIA clock is the same as the CPU (1=1).  The timer can be set for one of four intervals.
         * 
         * Setting the Timer - write a value or count (1..255) to the address of the desired interval
         *      0x294 (TIM1T) - 1 clock
         *      0x295 (TIM8T) - 8 clocks
         *      0x296 (TIM64T) - 64 clocks
         *      0x297 (T1024T) - 1024 clocks
         * PIA decrements the counter (one for each interval) down to 0. The counter is kept in 0x284 (INTIM)
         * The zero count is held for one interval and then the counter flips to 0xff and decrements each clock cycle.
         * 
         * RAM is located from 0x80 - 0xff.  The CPU uses 0xff and down for the stack and collisions are avoided only by hope.
         */

        private e6502 _cpu;

        private int _interval;
        private int _interval_count;
        private int _timer_count;

        public PIA(e6502 cpu)
        {
            //TODO: random value in INTIM needed?

            _cpu = cpu;

            // zero out the registers used
            //_cpu.memory[PIAAddress.INTIM] = 0;
            //_cpu.memory[PIAAddress.TIM1T] = 0;
            //_cpu.memory[PIAAddress.TIM8T] = 0;
            //_cpu.memory[PIAAddress.TIM64T] = 0;
            //_cpu.memory[PIAAddress.T1024T] = 0;

            _interval = 0;
            _interval_count = 0;
            _timer_count = 0;
        }

        public void Tick()
        {

            // decrement the timer
            _interval_count--;
            if( _interval_count == 0 )
            {
                // This interval has completed.  Decrement the timer count.
                _timer_count--;

                // If the timer count has completed 
            }
            


            //// check if a timer register has been written to
            //if( _cpu.memory[PIAAddress.TIM1T] != 0 )
            //{
            //    StartTimer(_cpu.memory[PIAAddress.TIM1T], 1);
            //}
            //else if(_cpu.memory[PIAAddress.TIM8T] != 0)
            //{
            //    StartTimer(_cpu.memory[PIAAddress.TIM8T], 8);
            //}
            //else if(_cpu.memory[PIAAddress.TIM64T] != 0)
            //{
            //    StartTimer(_cpu.memory[PIAAddress.TIM64T], 64);
            //}
            //else if(_cpu.memory[PIAAddress.T1024T] != 0)
            //{
            //    StartTimer(_cpu.memory[PIAAddress.T1024T], 1024);
            //}


        }

        private void StartTimer(int address, int interval)
        {
            //_cpu.memory[PIAAddress.INTIM] = _cpu.memory[address];
            //_cpu.memory[address] = 0;
            _interval = interval;
            _interval_count = 0;
        }


    }
}
