using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebForm.Startup))]

namespace WebForm
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapYuuko();
        }
    }
}
