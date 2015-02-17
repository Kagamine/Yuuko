using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class AutoTimeAttribute : SpecialFieldAttribute
    {
        public override object SetFieldValue(object Value, DetailPortFunction Method)
        {
            return DateTime.Now;
        }
    }
}
