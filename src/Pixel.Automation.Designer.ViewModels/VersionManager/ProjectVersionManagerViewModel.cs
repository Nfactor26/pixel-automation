using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Notifications.Notfications;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    public class ProjectVersionManagerViewModel: Caliburn.Micro.Screen, IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<ProjectVersionManagerViewModel>();

        private readonly IDataManagerFactory dataManagerFactory;
        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly INotificationManager notificationManager;
        private readonly ISerializer serializer;
        private readonly IProjectDataManager projectDataManager;      
        private readonly IApplicationDataManager applicationDataManager;
        private readonly IPrefabDataManager prefabDataManager;
        private readonly IApplicationFileSystem applicationFileSystem;
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
            IReferenceManagerFactory referenceManagerFactory, IApplicationDataManager applicationDataManager, IProjectDataManager projectDataManager,
            IDataManagerFactory dataManagerFactory, IPrefabDataManager prefabDataManager,
            ISerializer serializer, IApplicationFileSystem applicationFileSystem,
            IEventAggregator eventAggregator, INotificationManager notificationManager, ApplicationSettings applicationSettings)
        {
            this.DisplayName = "Manage Project Versions";
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;           
            this.applicationFileSystem = Guard.Argument(applicationFileSystem, nameof(applicationFileSystem)).NotNull().Value;
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.automationProject = Guard.Argument(automationProject, nameof(automationProject)).NotNull();
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.applicationFileSystem = Guard.Argument(applicationFileSystem, nameof(applicationFileSystem)).NotNull().Value;
            this.dataManagerFactory = Guard.Argument(dataManagerFactory, nameof(dataManagerFactory)).NotNull().Value;
            this.projectDataManager = Guard.Argument(projectDataManager, nameof(projectDataManager)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
          
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
                    VersionInfo newVersion = await projectVersionViewModel.CloneAsync(this.projectDataManager);
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
        /// Export the project data in to a zip file along with it's application ( controls + prefabs ) dependencies
        /// </summary>
        /// <returns></returns>
        public async Task ExportAsync(ProjectVersionViewModel projectVersionViewModel)
        {
            try
            {
                Guard.Argument(projectVersionViewModel, nameof(projectVersionViewModel)).NotNull();

                await this.projectDataManager.DownloadProjectDataFilesAsync(this.automationProject, projectVersionViewModel.ProjectVersion);

                IProjectFileSystem projectFileSystem = new ProjectFileSystem(serializer, applicationSettings);
                projectFileSystem.Initialize(this.automationProject, projectVersionViewModel.ProjectVersion);               
                var referenceManager = this.referenceManagerFactory.CreateReferenceManager(this.automationProject.ProjectId, projectVersionViewModel.ProjectVersion.ToString(), projectFileSystem);
                var projectAssetsDataManager = this.dataManagerFactory.CreateProjectAssetsDataManager(this.automationProject, projectVersionViewModel.ProjectVersion, projectFileSystem);

                //TODO : Download all thesee data in a single call
                await projectAssetsDataManager.DownloadAllFixturesAsync();
                await projectAssetsDataManager.DownloadAllTestsAsync();
                await projectAssetsDataManager.DownloadAllTestDataSourcesAsync();

                List<string> tags = new();
                foreach (var fixtureId in referenceManager.GetAllFixtures())
                {
                    string fixtureDirectory = Path.Combine(projectFileSystem.TestCaseRepository, fixtureId);
                    var testFixture = projectFileSystem.LoadFiles<TestFixture>(fixtureDirectory).Single();                
                    tags.Add(fixtureId);
                    foreach(var testCaseId in testFixture.TestCases)
                    {
                        tags.Add(testCaseId);
                    }
                }
                await projectAssetsDataManager.DownloadFilesWithTagsAsync(tags.ToArray());

                //Download latest versions of control for the applications used by version of project
                IEnumerable<string> applicationsUsed = referenceManager.GetControlReferences().References.Select(r => r.ApplicationId)
                .Union(referenceManager.GetPrefabReferences().References.Select(r => r.ApplicationId));
                foreach (var applicationId in applicationsUsed)
                {
                    await this.applicationDataManager.DownloadControlsAsync(applicationId);
                }

                //Download prefab versions used by the version of project
                await this.prefabDataManager.DownloadPrefabsAsync();
                IEnumerable<PrefabReference> prefabsUsed = referenceManager.GetPrefabReferences().References;
                foreach (var prefab in prefabsUsed)
                {
                    await this.prefabDataManager.DownloadPrefabDataAsync(prefab.ApplicationId, prefab.PrefabId, prefab.Version.ToString());
                }

                //zip project data, application data along with controls and prefabs
                var zipFileLocation = Path.Combine(applicationFileSystem.GetAutomationProjectDirectory(this.automationProject), 
                    $"{this.automationProject.Name.Replace(' ', '.')}.{projectVersionViewModel.ProjectVersion.ToString()}.zip");
                if (File.Exists(zipFileLocation))
                {
                    File.Delete(zipFileLocation);
                }
                using (var fileStream = new FileStream(zipFileLocation, FileMode.CreateNew))
                {
                    using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);

                    // Add the automation process files
                    zipArchive.CreateEntryFromFile(projectFileSystem.ProjectFile, Path.GetRelativePath(Environment.CurrentDirectory, projectFileSystem.ProjectFile));
                    foreach (var file in Directory.GetFiles(projectFileSystem.WorkingDirectory, "*.*", new EnumerationOptions() { RecurseSubdirectories = true }))
                    {
                        zipArchive.CreateEntryFromFile(file, Path.GetRelativePath(Environment.CurrentDirectory, file));
                    }

                    //Add application files along with control and prefabs belonging to application
                    foreach (var applicationId in applicationsUsed)
                    {
                        string applicationsDirectory = Path.Combine(this.applicationFileSystem.GetApplicationsDirectory(), applicationId);
                        foreach (var file in Directory.GetFiles(applicationsDirectory, "*.*", new EnumerationOptions() { RecurseSubdirectories = true }))
                        {
                            zipArchive.CreateEntryFromFile(file, Path.GetRelativePath(Environment.CurrentDirectory, file));
                        }
                    }
                }
                Process.Start("explorer", Path.GetDirectoryName(zipFileLocation));
                logger.Information("Exported project data to zip file : '{0}'", zipFileLocation);
                await CloseAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to clone version : '{0}'", projectVersionViewModel?.Version);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        /// <summary>
        /// Open selected version of project to edit
        /// </summary>
        /// <param name="projectVersionViewModel">Version of the project to edit</param>
        /// <returns></returns>
        public async Task OpenForEditAsync(ProjectVersionViewModel projectVersionViewModel)
        {
            var openProjectVersionNotification = new OpenProjectVersionForEditNotification(this.automationProject, projectVersionViewModel.ProjectVersion);
            await this.eventAggregator.PublishOnBackgroundThreadAsync(openProjectVersionNotification);
            await CloseAsync();
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
