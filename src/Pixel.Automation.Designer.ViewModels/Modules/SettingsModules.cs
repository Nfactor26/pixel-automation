using Microsoft.Extensions.Configuration;
using Ninject.Modules;
using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class SettingsModules : NinjectModule
    {
        public override void Load()
        {

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            Kernel.Bind<IConfiguration>().ToConstant(config);

            var applicationSettings = config.GetSection("applicationSettings").Get<ApplicationSettings>();
            Kernel.Bind<ApplicationSettings>().ToConstant(applicationSettings);
            var userSettings = config.GetSection("userSettings").Get<UserSettings>();
            Kernel.Bind<UserSettings>().ToConstant(userSettings);

        }
    }
}
