using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class TakeAttribute : Attribute
    {
        public string requestKey;

        public int defaultTakeCount;

        public TakeAttribute(int TakeCount)
        {
            defaultTakeCount = TakeCount;
        }

        public TakeAttribute(string RequestKey)
        {
            requestKey = RequestKey;
        }

        public TakeAttribute(string RequestKey, int DefaultTakeCount)
        {
            requestKey = RequestKey;
            defaultTakeCount = TakeCount;
        }
    }
}
