using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(Pixel.Automation.Web.Scrapper.Startup))]
namespace Pixel.Automation.Web.Scrapper
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            //app.UseCors(CorsOptions.AllowAll);         


            //try
            //{
            //    GlobalHost.HubPipeline.AddModule(new ErrorHandlingPipelineModule());
            //}
            //catch (System.Exception)
            //{


            //}


            //app.UseFileServer(new FileServerOptions()
            //{
            //    RequestPath = new PathString("/Scripts"),
            //    EnableDirectoryBrowsing = true,
            //});

            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.Map("/signalr", map =>
            {
                // Setup the CORS middleware to run before SignalR.
                // By default this will allow all origins. You can 
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting line below.
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    EnableJSONP = true,
                    EnableDetailedErrors = true                   
                };
                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch already runs under the "/signalr"
                // path.
                map.RunSignalR(hubConfiguration);
            });
        }
    }
}
