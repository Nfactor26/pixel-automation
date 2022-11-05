using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client.Interfaces;
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
        private readonly IPrefabDataManager prefabDataManager;
        private readonly PrefabProject prefabProject;
        private readonly ApplicationSettings applicationSettings;

        public BindableCollection<PrefabVersionViewModel> AvailableVersions { get; set; } = new BindableCollection<PrefabVersionViewModel>();
       

        public PrefabVersionManagerViewModel(PrefabProject prefabProject, IWorkspaceManagerFactory workspaceManagerFactory,
            IReferenceManagerFactory referenceManagerFactory, ISerializer serializer,
            IPrefabDataManager prefabDataManager, ApplicationSettings applicationSettings)
        {
            this.DisplayName = "Manage Prefab Versions";
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;           
            this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
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
                PrefabVersion newVersion = await prefabVersionViewModel.CloneAsync(this.prefabDataManager);
                IPrefabFileSystem fileSystem = new PrefabFileSystem(serializer, applicationSettings);
                fileSystem.Initialize(this.prefabProject, newVersion);

                //After cloning, we mark the curret version as published
                if (!prefabVersionViewModel.IsPublished)
                {
                    await prefabVersionViewModel.PublishAsync(this.workspaceManagerFactory, this.prefabDataManager);
                    logger.Information($"Version {prefabVersionViewModel.Version} for Prefab : {this.prefabProject.PrefabName} is published now.");
                }             
                                         
                this.AvailableVersions.Add(new PrefabVersionViewModel(this.prefabProject, newVersion, fileSystem, referenceManagerFactory));   
                
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
                    await prefabVersionViewModel.PublishAsync(this.workspaceManagerFactory, this.prefabDataManager);                   
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
