using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Test.Startup))]

namespace Test
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapYuuko();
        }
    }
}
