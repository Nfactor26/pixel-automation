using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Reference.Manager.Contracts;
using Serilog;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    public class PrefabVersionManagerViewModel : Screen , IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<PrefabVersionManagerViewModel>();

        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly PrefabProject prefabProject;
        private readonly ApplicationSettings applicationSettings;

        public BindableCollection<PrefabVersionViewModel> AvailableVersions { get; set; } = new BindableCollection<PrefabVersionViewModel>();
       

        public PrefabVersionManagerViewModel(PrefabProject prefabProject, IWorkspaceManagerFactory workspaceManagerFactory, IReferenceManagerFactory referenceManagerFactory,
            ISerializer serializer, IApplicationDataManager applicationDataManager,  ApplicationSettings applicationSettings)
        {
            this.DisplayName = "Manage Prefab Versions";
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.prefabProject = Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            foreach (var version in this.prefabProject.AvailableVersions)
            {
                IPrefabFileSystem fileSystem = new PrefabFileSystem(serializer, applicationSettings);
                AvailableVersions.Add(new PrefabVersionViewModel(this.prefabProject, version, fileSystem, referenceManagerFactory));
            }
        }

        /// <summary>
        /// Create a copy of selected version and mark the selected version as published.
        /// </summary>
        /// <returns></returns>
        public async Task CloneAndPublishAsync(PrefabVersionViewModel prefabVersionViewModel)
        {
            try
            {
                logger.Information($"Trying to clone version {prefabVersionViewModel.Version} for Prefab : {this.prefabProject.PrefabName}");

                //Create a new active version from selected version
                PrefabVersion newVersion = prefabVersionViewModel.Clone();
                IPrefabFileSystem fileSystem = new PrefabFileSystem(serializer, applicationSettings);
                fileSystem.Initialize(this.prefabProject, newVersion);

                //Publish the selected version
                if(!prefabVersionViewModel.IsPublished)
                {
                    prefabVersionViewModel.Publish(this.workspaceManagerFactory);
                    logger.Information($"Version {prefabVersionViewModel.Version} for Prefab : {this.prefabProject.PrefabName} is published now.");
                }

                int indexToInsert = 0;
                foreach (var version in this.prefabProject.AvailableVersions.Select(s => s.Version))
                {                   
                    if (version < newVersion.Version)
                    {
                        indexToInsert++;
                        continue;
                    }                          
                    break;
                }

                this.prefabProject.AvailableVersions.Insert(indexToInsert, newVersion);
                serializer.Serialize<PrefabProject>(fileSystem.PrefabDescriptionFile, this.prefabProject);

                await this.applicationDataManager.AddOrUpdatePrefabAsync(this.prefabProject, new PrefabVersion(prefabVersionViewModel.Version) { IsActive = false });
                await this.applicationDataManager.AddOrUpdatePrefabDataFilesAsync(this.prefabProject, new PrefabVersion(prefabVersionViewModel.Version) { IsActive = false });
                await this.applicationDataManager.AddOrUpdatePrefabDataFilesAsync(this.prefabProject, newVersion);

                this.AvailableVersions.Insert(indexToInsert, new PrefabVersionViewModel(this.prefabProject, newVersion, fileSystem, referenceManagerFactory));                
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Create a copy of selected version and mark the selected version as published.
        /// </summary>
        /// <returns></returns>
        public async Task PublishAsync(PrefabVersionViewModel prefabVersionViewModel)
        {
            try
            {
                if (prefabVersionViewModel.CanPublish)
                {
                    prefabVersionViewModel.Publish(this.workspaceManagerFactory);
                    await this.applicationDataManager.AddOrUpdatePrefabAsync(this.prefabProject, new PrefabVersion(prefabVersionViewModel.Version) { IsActive = false });
                    await this.applicationDataManager.AddOrUpdatePrefabDataFilesAsync(this.prefabProject, new PrefabVersion(prefabVersionViewModel.Version) { IsActive = false });
                    logger.Information($"Version {prefabVersionViewModel.Version} for project : {this.prefabProject.PrefabName} is published now.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task CloseAsync()
        {
            await this.TryCloseAsync(false);
        }
    }
}
