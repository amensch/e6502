using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Untari.Console
{
    public class SystemConsole
    {

        /*
         * The system console is made up of the following devices:
         * 
         * The 6507 CPU (Uses lower 12 bits only)
         * The 6532 RIOT (C/S if A7=1 and A12=0) RAM select (A9=0)
         *      The RIOT is $0280-029F
         * The TIA  (C/S if A7=0 and A12=0). Uses lower 6 bits only.
         * The cartridge: (C/S if A12=1)
         * Console settings
         */


    }
}
