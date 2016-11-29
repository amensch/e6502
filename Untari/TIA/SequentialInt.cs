using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Untari.TIA
{
    /// <summary>
    /// This class is an integer which automatically rolls over when the 
    /// defined upper limit is reached.
    /// </summary>
    public class SequentialInt
    {

        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }
        private int currentValue;

        /// <summary>
        /// Define min and max value inclusive. Initial value is set to min.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public SequentialInt(int min, int max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException("max", "Min value cannot be greater than or equal to max");

            MinValue = min;
            MaxValue = max;
            currentValue = min;
        }

        public int Value
        {
            get
            {
                return currentValue;
            }
            set
            {
                if(value < MinValue)
                    throw new ArgumentOutOfRangeException("Current value cannot be less than the min");

                if(value > MaxValue)
                    throw new ArgumentOutOfRangeException("Current value cannot be greater than the max");

                currentValue = value;
            }
        }
    }
}
