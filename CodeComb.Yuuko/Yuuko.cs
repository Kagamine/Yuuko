using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Web.SessionState;

namespace CodeComb.Yuuko
{
    public static class Yuuko
    {
        public static void RegisterSessonControl(HttpSessionState Session)
        {
            Session["YuukoToken"] = Guid.NewGuid().ToString();
        }
    }
}
