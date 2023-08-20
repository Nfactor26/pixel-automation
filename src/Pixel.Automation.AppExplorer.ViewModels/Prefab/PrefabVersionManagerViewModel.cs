using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System.Diagnostics;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    public class PrefabVersionManagerViewModel : Screen , IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<PrefabVersionManagerViewModel>();

        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly ISerializer serializer;    
        private readonly IPrefabDataManager prefabDataManager;
        private readonly INotificationManager notificationManager;
        private readonly PrefabProject prefabProject;
        private readonly ApplicationSettings applicationSettings;

        /// <summary>
        /// Available version of Prefab
        /// </summary>
        public BindableCollection<PrefabVersionViewModel> AvailableVersions { get; set; } = new BindableCollection<PrefabVersionViewModel>();
       
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <param name="workspaceManagerFactory"></param>
        /// <param name="referenceManagerFactory"></param>
        /// <param name="serializer"></param>
        /// <param name="prefabDataManager"></param>
        /// <param name="applicationSettings"></param>
        public PrefabVersionManagerViewModel(PrefabProject prefabProject, IWorkspaceManagerFactory workspaceManagerFactory,
            IReferenceManagerFactory referenceManagerFactory, ISerializer serializer,
            IPrefabDataManager prefabDataManager, INotificationManager notificationManager, ApplicationSettings applicationSettings)
        {
            this.DisplayName = "Manage Prefab Versions";
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;           
            this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.prefabProject = Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            foreach (var version in this.prefabProject.AvailableVersions)
            {
                IPrefabFileSystem fileSystem = new PrefabFileSystem(serializer, applicationSettings);
                AvailableVersions.Add(new PrefabVersionViewModel(this.prefabProject, version, fileSystem, referenceManagerFactory));
            }
        }

        /// <summary>
        /// Set the state of version as published.
        /// </summary>
        /// <returns></returns>
        public async Task PublishAsync(PrefabVersionViewModel prefabVersionViewModel)
        {
            Guard.Argument(prefabVersionViewModel, nameof(prefabVersionViewModel)).NotNull();
            using (var activity = Telemetry.DefaultSource.StartActivity(nameof(PublishAsync), ActivityKind.Internal))
            {
                try
                {
                    if (!prefabVersionViewModel.IsPublished)
                    {
                        bool isLatestActieVersion = prefabProject.LatestActiveVersion.Version.Equals(prefabVersionViewModel.Version);
                        await prefabVersionViewModel.PublishAsync(this.workspaceManagerFactory, this.prefabDataManager);
                        logger.Information("Version {0} for project : {1} is published now.", prefabVersionViewModel.Version, this.prefabProject.PrefabName);
                        if (isLatestActieVersion)
                        {
                            await CloneAsync(prefabVersionViewModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to publish verion : {0} of prefab : {1}", prefabVersionViewModel.Version, prefabVersionViewModel.PrefabName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }           
        }

        /// <summary>
        /// Create a copy of selected version. Only published versions can be cloned.
        /// </summary>
        /// <returns></returns>
        public async Task CloneAsync(PrefabVersionViewModel prefabVersionViewModel)
        {
            Guard.Argument(prefabVersionViewModel, nameof(prefabVersionViewModel)).NotNull();
            using (var activity = Telemetry.DefaultSource.StartActivity(nameof(CloneAsync), ActivityKind.Internal))
            {
                try
                {
                    if (prefabVersionViewModel.IsPublished)
                    {
                        activity?.SetTag("FromVersion", prefabVersionViewModel.Version.ToString());
                        PrefabVersion newVersion = await prefabVersionViewModel.CloneAsync(this.prefabDataManager);
                        IPrefabFileSystem fileSystem = new PrefabFileSystem(serializer, applicationSettings);
                        fileSystem.Initialize(this.prefabProject, newVersion);
                        this.AvailableVersions.Add(new PrefabVersionViewModel(this.prefabProject, newVersion, fileSystem, referenceManagerFactory));
                        logger.Information("Version {0} for project : {1} is cloned from {2}.", newVersion.Version, this.prefabProject.PrefabName, prefabVersionViewModel.Version);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to create a new verion of prefab : {0} from version : {1}", prefabVersionViewModel.PrefabName, prefabVersionViewModel.Version);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }            
        }

        /// <summary>
        /// Close Prefab version manager screen
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            await this.TryCloseAsync(false);
        }
    }
}
