using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class BindingAttribute : Attribute
    {
        public string _DataSource;
        public BindingAttribute(string DataSource)
        {
            _DataSource = DataSource;
        }
    }
}
