using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{

    public class PrefabReferenceManagerViewModel : Caliburn.Micro.Screen, IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<PrefabReferenceManagerViewModel>();
        private readonly IReferenceManager referenceManager;
        private readonly INotificationManager notificationManager;
        public readonly PrefabReferences prefabReferences;

        /// <summary>
        /// Collection of <see cref="PrefabReference"/> usesd in the project
        /// </summary>
        public ObservableCollection<PrefabReferenceViewModel> PrefabReferences { get; private set; } = new();

        /// <summary>
        /// Filter text to filter for prefabs
        /// </summary>
        string filterText = string.Empty;
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                var fixtureView = CollectionViewSource.GetDefaultView(PrefabReferences);
                fixtureView.Refresh();
                NotifyOfPropertyChange(() => FilterText);
            }
        }

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
            CreateDefaultView();
        }

        /// <summary>
        /// Setup the collection view with grouping and sorting
        /// </summary>
        private void CreateDefaultView()
        {
            var fixtureGroupedItems = CollectionViewSource.GetDefaultView(PrefabReferences);       
            fixtureGroupedItems.SortDescriptions.Add(new SortDescription(nameof(PrefabReferenceViewModel.PrefabName), ListSortDirection.Ascending));
            fixtureGroupedItems.Filter = new Predicate<object>((a) =>
            {
                if (a is PrefabReferenceViewModel prefabReference)
                {
                    return prefabReference.PrefabName.Contains(FilterText) || prefabReference.PrefabName.Equals(FilterText);
                }
                return true;
            });
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
