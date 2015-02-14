using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class SelectAttribute : Attribute
    {
        public string selector;

        public object[] values;

        public SelectAttribute(string Selector, params object[] Values)
        {
            selector = Selector.Replace("'", "\"");
            values = Values;
        }
    }
}
