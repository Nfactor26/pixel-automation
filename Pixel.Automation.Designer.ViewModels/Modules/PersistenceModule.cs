using Ninject.Modules;
using Pixel.Persistence.Services.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Automation.Designer.ViewModels.Modules
{    
    public class PersistenceModule : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IApplicationRepositoryClient>().To<ApplicationRepositoryClient>();
        }
    }
}
