using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace CodeComb.Yuuko.Schema
{
    public abstract class AccessToAttribute : Attribute
    {
        /// <summary>
        /// override this method to check authority of this port;
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public virtual bool AccessCore(IPrincipal User)
        {
            return true;
        }
    }
}
