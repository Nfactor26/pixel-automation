using Dawn;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Reference.Manager;
using Pixel.Scripting.Reference.Manager.Contracts;
using System.Collections.ObjectModel;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{

    public class PrefabReferenceManagerViewModel : Caliburn.Micro.Screen, IVersionManager
    {
        private readonly IReferenceManager referenceManager;
        public readonly PrefabReferences prefabReferences;

        public ObservableCollection<PrefabReferenceViewModel> PrefabReferences { get; private set; } = new();

        public PrefabReferenceManagerViewModel(IPrefabDataManager prefabDataManager, IReferenceManager referenceManager)
        {
            this.DisplayName = "Manage Prefab References";
            this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;           
            this.prefabReferences = this.referenceManager.GetPrefabReferences();

            var applications = this.prefabReferences.References.Select(r => r.ApplicationId).Distinct();
            foreach (var application in applications)
            {
                foreach (var prefab in prefabDataManager.GetAllPrefabs(application))
                {
                    if (this.prefabReferences.HasReference(prefab))
                    {
                        var prefabReference = this.prefabReferences.GetPrefabReference(prefab.PrefabId);
                        this.PrefabReferences.Add(new PrefabReferenceViewModel(prefab, prefabReference));
                    }
                }
            }
        }

        public async Task SaveAsync()
        {
            var modifiedReferences = this.PrefabReferences.Where(p => p.IsDirty).Select(p => p.prefabReference);
            if(modifiedReferences.Any())
            {
                await this.referenceManager.UpdatePrefabReferencesAsync(modifiedReferences);
            }
            await this.TryCloseAsync(true);
        }

        public async Task CloseAsync()
        {
            await this.TryCloseAsync(true);
        }
    }
}
