using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using System.Collections.ObjectModel;
using System.IO;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{

    public class PrefabReferenceManagerViewModel : Caliburn.Micro.Screen, IVersionManager
    {
        private readonly IProjectFileSystem projectFileSystem;
        public readonly PrefabReferences prefabReferences;

        public ObservableCollection<PrefabReferenceViewModel> PrefabReferences { get; private set; } = new();

        public PrefabReferenceManagerViewModel(IProjectFileSystem projectFileSystem, IApplicationDataManager applicationDataManager)
        {
            this.DisplayName = "Manage Prefab References";
            this.projectFileSystem = Guard.Argument(projectFileSystem).NotNull().Value;
            this.prefabReferences = this.projectFileSystem.LoadFile<PrefabReferences>(projectFileSystem.PrefabReferencesFile);

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
            this.projectFileSystem.SaveToFile<PrefabReferences>(prefabReferences, Path.GetDirectoryName(projectFileSystem.PrefabReferencesFile), Path.GetFileName(projectFileSystem.PrefabReferencesFile));
            await this.TryCloseAsync(true);
        }

        public async Task CloseAsync()
        {
            await this.TryCloseAsync(true);
        }
    }
}
