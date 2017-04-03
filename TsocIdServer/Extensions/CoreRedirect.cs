using System.Threading.Tasks;
using Microsoft.Owin;

namespace TsocIdServer.Extensions
{
    public class CoreRedirect : OwinMiddleware
    {
        public CoreRedirect(OwinMiddleware next) : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            if (context.Request.Path.Value == "/core") { 
            context.Request.Path = new PathString("/identity/AuthServices/Acs");
            }
            return Next.Invoke(context);
        }
    }
}
