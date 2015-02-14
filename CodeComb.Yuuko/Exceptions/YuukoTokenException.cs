using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Exceptions
{
    public class YuukoTokenException : Exception
    {
        public YuukoTokenException() : base("The Yuuko Token is illegal.")
        {
        }
    }
}
