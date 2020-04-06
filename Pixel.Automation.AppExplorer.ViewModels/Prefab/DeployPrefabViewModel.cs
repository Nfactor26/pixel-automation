using Caliburn.Micro;
using Pixel.Automation.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    public class DeployPrefabViewModel : Screen
    {
        private readonly PrefabDescription prefabDescription;

        private List<PrefabVersionViewModel> availableVersions = new List<PrefabVersionViewModel>();
        public List<PrefabVersionViewModel> AvailableVersions
        {
            get
            {
                if(prefabDescription.NonDeployedVersions.Any() && !availableVersions.Any())
                {
                    foreach(var prefabVersion in prefabDescription.NonDeployedVersions)
                    {
                        availableVersions.Add(new PrefabVersionViewModel(prefabDescription, prefabVersion));
                    }
                }
                return availableVersions;
            }
        }
  
        public DeployPrefabViewModel(PrefabDescription prefabDescription)
        {
            this.prefabDescription = prefabDescription;            
        }       

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            return base.TryCloseAsync(true);
        }
    }
}
