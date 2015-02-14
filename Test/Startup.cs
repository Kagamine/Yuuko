using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Owin;
using Owin;
using CodeComb.Yuuko;
using Test;

[assembly: OwinStartup(typeof(Test.Startup))]

namespace Test
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            app.MapYuuko();
        }
    }
}
