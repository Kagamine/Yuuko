using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class OrderByAttribute : Attribute
    {
        public string ordering;

        public object[] values;

        public OrderByAttribute(string Ordering, params object[] Values)
        {
            ordering = Ordering;
            values = Values;
        }
    }
}
