using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{

    public class PrefabReferenceManagerViewModel : Screen, IVersionManager
    {
        private readonly IFileSystem projectFileSystem;
        public readonly PrefabReferences prefabReferences;

        public ObservableCollection<PrefabReferenceViewModel> PrefabReferences { get; private set; } = new();

        public PrefabReferenceManagerViewModel(IFileSystem projectFileSystem, IApplicationDataManager applicationDataManager)
        {
            this.DisplayName = "Manage Prefab References";
            this.projectFileSystem = Guard.Argument(projectFileSystem).NotNull().Value;
            this.prefabReferences = this.projectFileSystem.LoadFile<PrefabReferences>(Path.Combine(projectFileSystem.WorkingDirectory,
                "PrefabReferences.ref"));

            var applications = this.prefabReferences.References.Select(r => r.ApplicationId).Distinct();
            foreach (var application in applications)
            {
                foreach (var prefab in applicationDataManager.GetAllPrefabs(application))
                {
                    if (this.prefabReferences.HasReference(prefab))
                    {
                        var prefabReference = this.prefabReferences.GetPrefabReference(prefab.PrefabId);
                        this.PrefabReferences.Add(new PrefabReferenceViewModel(prefab, prefabReference));
                    }
                }
            }
        }

        public async void SaveAsync()
        {        
            this.projectFileSystem.SaveToFile<PrefabReferences>(prefabReferences, projectFileSystem.WorkingDirectory, "PrefabReferences.ref");
            await this.TryCloseAsync(true);
        }

        public async Task CloseAsync()
        {
            await this.TryCloseAsync(true);
        }
    }
}
