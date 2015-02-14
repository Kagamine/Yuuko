using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CodeComb.Yuuko.Schema
{
    public class DataSourceAttribute : Attribute
    {
        public object _DataSource;
        public DataSourceAttribute (object DataSource)
        {
            _DataSource = DataSource;
        }
    }
}
