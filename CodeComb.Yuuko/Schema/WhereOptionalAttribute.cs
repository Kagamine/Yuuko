using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class WhereOptionalAttribute : Attribute
    {
        public string predicate;

        public Type[] types;

        public object[] values;

        public WhereOptionalAttribute(string Predicate, params Type[] Types)
        {
            predicate = Predicate;
            types = Types;
        }
    }
}
