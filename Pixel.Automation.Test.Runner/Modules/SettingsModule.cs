using Microsoft.Extensions.Configuration;
using Ninject.Modules;
using Pixel.Automation.Core;

namespace Pixel.Automation.Test.Runner.Modules
{
    public class SettingsModule : NinjectModule
    {
        public override void Load()
        {

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            Kernel.Bind<IConfiguration>().ToConstant(config);

            var applicationSettings = config.GetSection("applicationSettings").Get<ApplicationSettings>();
            Kernel.Bind<ApplicationSettings>().ToConstant(applicationSettings);           
        }
    }
}
