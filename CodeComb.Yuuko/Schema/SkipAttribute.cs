using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class SkipAttribute : Attribute
    {
        public string requestKey;

        public int defaultSkipCount;

        public SkipAttribute(string RequestKey)
        {
            requestKey = RequestKey;
            defaultSkipCount = 0;
        }

        public SkipAttribute(int SkipCount)
        {
            defaultSkipCount = SkipCount;
        }

        public SkipAttribute(string RequestKey, int DefaultSkipCount)
        {
            requestKey = RequestKey;
            defaultSkipCount = DefaultSkipCount;
        }
    }
}
