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

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    public class ProjectVersionManagerViewModel: Caliburn.Micro.Screen, IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<ProjectVersionManagerViewModel>();

        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly INotificationManager notificationManager;
        private readonly ISerializer serializer;
        private readonly IProjectDataManager projectDataManager;
        private readonly AutomationProject automationProject;
        private readonly ApplicationSettings applicationSettings;

        private bool wasPublishedOrCloned;

        public BindableCollection<ProjectVersionViewModel> AvailableVersions { get; set; } = new BindableCollection<ProjectVersionViewModel>();
       
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="automationProject"></param>
        /// <param name="workspaceManagerFactory"></param>
        /// <param name="referenceManagerFactory"></param>
        /// <param name="serializer"></param>
        /// <param name="projectDataManager"></param>
        /// <param name="applicationSettings"></param>
        public ProjectVersionManagerViewModel(AutomationProject automationProject, IWorkspaceManagerFactory workspaceManagerFactory,
            IReferenceManagerFactory referenceManagerFactory, ISerializer serializer, IProjectDataManager projectDataManager,
            INotificationManager notificationManager, ApplicationSettings applicationSettings)
        {
            this.DisplayName = "Manage Project Versions";
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.projectDataManager = Guard.Argument(projectDataManager, nameof(projectDataManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.automationProject = Guard.Argument(automationProject, nameof(automationProject)).NotNull();
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            foreach (var version in this.automationProject.AvailableVersions)
            {
                IProjectFileSystem projectFileSystem = new ProjectFileSystem(serializer, applicationSettings);
                AvailableVersions.Add(new ProjectVersionViewModel(this.automationProject, version, projectFileSystem, referenceManagerFactory));
            }
        }

        /// <summary>
        /// Set the state of version as published.
        /// </summary>
        /// <returns></returns>
        public async Task PublishAsync(ProjectVersionViewModel projectVersionViewModel)
        {
            try
            {
                Guard.Argument(projectVersionViewModel, nameof(projectVersionViewModel)).NotNull();
                if (!projectVersionViewModel.IsPublished)
                {
                    bool isLatestActieVersion = automationProject.LatestActiveVersion.Version.Equals(projectVersionViewModel.Version);
                    await projectVersionViewModel.PublishAsync(this.workspaceManagerFactory, this.projectDataManager);
                    if (isLatestActieVersion)
                    {
                        await CloneAsync(projectVersionViewModel);
                    }
                    this.wasPublishedOrCloned = true;
                    logger.Information("Version {0} for project : {1} is published now.", projectVersionViewModel.Version, this.automationProject.Name);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to publish version : '{0}'", projectVersionViewModel?.Version);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }


        /// <summary>
        /// Create a copy of selected version. Only published versions can be cloned.
        /// </summary>
        /// <returns></returns>
        public async Task CloneAsync(ProjectVersionViewModel projectVersionViewModel)
        {
            try
            {
                Guard.Argument(projectVersionViewModel, nameof(projectVersionViewModel)).NotNull();
                if (projectVersionViewModel.IsPublished)
                {
                    //Create a new active version from selected version
                    ProjectVersion newVersion = await projectVersionViewModel.CloneAsync(this.projectDataManager);
                    IProjectFileSystem projectFileSystem = new ProjectFileSystem(serializer, this.applicationSettings);
                    projectFileSystem.Initialize(this.automationProject, newVersion);
                    this.AvailableVersions.Add(new ProjectVersionViewModel(this.automationProject, newVersion, projectFileSystem, referenceManagerFactory));
                    this.wasPublishedOrCloned = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to clone version : '{0}'", projectVersionViewModel?.Version);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }           
        }

        /// <summary>
        /// Close Project version manager screen
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            await this.TryCloseAsync(this.wasPublishedOrCloned);
        }
    }
}
