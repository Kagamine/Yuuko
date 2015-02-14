using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class SingleByAttribute : Attribute
    {
        public string requestKey;

        public SingleByAttribute()
        {
            requestKey = "$key";
        }

        public SingleByAttribute(string RequestKey)
        {
            requestKey = RequestKey;
        }
    }
}
