using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public abstract class SpecialFieldAttribute : Attribute
    {
        public virtual object SetFieldValue(object Value, DetailPortFunction Method)
        {
            return Value;
        }
    }
}
