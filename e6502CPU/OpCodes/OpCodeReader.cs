using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e6502CPU
{
    public class OpCodeReader : IEnumerable<string>
    {
        private List<string> oplist;

        public OpCodeReader()
        {
            string[] orglist = e6502CPU.Properties.Resources.OpcodeList.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            oplist = new List<string>();

            // Remove the first two entries as well as blank and null lines
            for( int ii=0; ii <= orglist.GetUpperBound(0); ii++)
            {
                if ((ii > 1) && orglist[ii] != null && orglist[ii].Length > 0)
                    oplist.Add(orglist[ii]);
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)oplist).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)oplist).GetEnumerator();
        }
    }
}
