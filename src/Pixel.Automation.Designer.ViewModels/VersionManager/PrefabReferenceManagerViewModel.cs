using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System.Collections.ObjectModel;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{

    public class PrefabReferenceManagerViewModel : Caliburn.Micro.Screen, IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<PrefabReferenceManagerViewModel>();
        private readonly IReferenceManager referenceManager;
        private readonly INotificationManager notificationManager;
        public readonly PrefabReferences prefabReferences;

        public ObservableCollection<PrefabReferenceViewModel> PrefabReferences { get; private set; } = new();

        public PrefabReferenceManagerViewModel(IPrefabDataManager prefabDataManager, IReferenceManager referenceManager,
            INotificationManager notificationManager)
        {
            this.DisplayName = "Manage Prefab References";
            this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
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
            try
            {
                var modifiedReferences = this.PrefabReferences.Where(p => p.IsDirty).Select(p => p.prefabReference);
                if (modifiedReferences.Any())
                {
                    await this.referenceManager.UpdatePrefabReferencesAsync(modifiedReferences);
                }
                await this.TryCloseAsync(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to save changes to prefab references");
                await notificationManager.ShowErrorNotificationAsync(ex.Message);
            }
        }

        public async Task CloseAsync()
        {
            await this.TryCloseAsync(true);
        }
    }
}
