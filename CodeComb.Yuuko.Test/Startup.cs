using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CodeComb.Yuuko.Test.Startup))]

namespace CodeComb.Yuuko.Test
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapYuuko();
        }
    }
}
