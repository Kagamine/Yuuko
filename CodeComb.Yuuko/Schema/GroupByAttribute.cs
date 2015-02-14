using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class GroupByAttribute
    {
        public string keySelector;

        public string elementSelector;

        public object[] values;

        public GroupByAttribute(string KeySelector, string ElementSelector, params object[] Values)
        {
            keySelector = KeySelector;
            elementSelector = ElementSelector;
            values = Values;
        }
    }
}
