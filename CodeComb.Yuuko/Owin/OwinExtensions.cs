using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder MapYuuko(this IAppBuilder builder)
        {
            return builder.Map("/yuuko", app=> {
                app.Run(async context => {
                    context.Response.ContentType = "text/plain";
                    context.Response.WriteAsync("test");
                });
            });
        }
    }
}
