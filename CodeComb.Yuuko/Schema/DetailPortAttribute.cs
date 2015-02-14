using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public enum DetailPortFunction
    {
        Edit,
        Delete,
        Insert
    }
    public class DetailPortAttribute : Attribute
    {
        public DetailPortFunction[] detailPortFunctions;

        public DetailPortAttribute(params DetailPortFunction[] DetailPortFunction)
        {
            detailPortFunctions = DetailPortFunction;
        }
    }
}
