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

        public string requestKey;

        public PagingAttribute(int PageSize)
        {
            pageSize = PageSize;
            requestKey = "p";
        }
    }
}
