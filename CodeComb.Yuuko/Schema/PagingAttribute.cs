using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Schema
{
    public class PagingAttribute : Attribute
    {
        public int pageSize;

        public PagingAttribute(int PageSize)
        {
            pageSize = PageSize;
        }
    }
}
