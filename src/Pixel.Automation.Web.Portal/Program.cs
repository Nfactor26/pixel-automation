using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Pixel.Automation.Web.Portal.Security;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Security;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
          
            builder.Services.AddHttpClient("Pixel.Automation.Web.Portal", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
              .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Pixel.Automation.Web.Portal"));
          
            builder.Services.AddHttpClient<ITestResultService, TestResultService>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
               .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>(); ;
            builder.Services.AddHttpClient<ITestSessionService, TestSessionService>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                 .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>(); ;
            builder.Services.AddHttpClient<ITestStatisticsService, TestStatisticsService>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                 .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>(); ;
            builder.Services.AddHttpClient<IProjectStatisticsService, ProjectStatisticsService>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                 .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>(); ;

            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("PixelDashboardUI", options.ProviderOptions);              
                options.UserOptions.RoleClaim = "role";             
            })
            .AddAccountClaimsPrincipalFactory<IdentityClaimsPrincipalFactory<RemoteUserAccount>>();


            builder.Services.AddAuthorizationCore(options =>
            {              
                options.AddPolicy(Policies.DashboardUserPolicy, policy => policy.RequireRole(Roles.DashboardUserRole));
            });

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
            });

            await builder.Build().RunAsync();
        }
    }
}
