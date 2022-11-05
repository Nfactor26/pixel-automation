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

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    public class ProjectVersionManagerViewModel: Caliburn.Micro.Screen, IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<ProjectVersionManagerViewModel>();

        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly ISerializer serializer;
        private readonly IProjectDataManager projectDataManager;
        private readonly AutomationProject automationProject;
        private readonly ApplicationSettings applicationSettings;

        private bool wasPublished;

        public BindableCollection<ProjectVersionViewModel> AvailableVersions { get; set; } = new BindableCollection<ProjectVersionViewModel>();
       
        public ProjectVersionManagerViewModel(AutomationProject automationProject, IWorkspaceManagerFactory workspaceManagerFactory,
            IReferenceManagerFactory referenceManagerFactory, ISerializer serializer, IProjectDataManager projectDataManager, ApplicationSettings applicationSettings)
        {
            this.DisplayName = "Manage Project Versions";
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.projectDataManager = Guard.Argument(projectDataManager, nameof(projectDataManager)).NotNull().Value;
            this.automationProject = Guard.Argument(automationProject, nameof(automationProject)).NotNull();
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            foreach (var version in this.automationProject.AvailableVersions)
            {
                IProjectFileSystem projectFileSystem = new ProjectFileSystem(serializer, applicationSettings);
                AvailableVersions.Add(new ProjectVersionViewModel(this.automationProject, version, projectFileSystem, referenceManagerFactory));
            }
        }

        /// <summary>
        /// Create a copy of selected version and mark the selected version as Published.
        /// </summary>
        /// <returns></returns>
        public async Task CloneAndPublishAsync(ProjectVersionViewModel projectVersionViewModel)
        {
            try
            {
                logger.Information($"Trying to clone version {projectVersionViewModel.Version} for Project : {this.automationProject.Name}");

                //Create a new active version from selected version
                ProjectVersion newVersion = await projectVersionViewModel.CloneAsync(this.projectDataManager);                

                IProjectFileSystem projectFileSystem = new ProjectFileSystem(serializer, this.applicationSettings);
                projectFileSystem.Initialize(this.automationProject, newVersion);

                //After cloning, we mark the curret version as published
                if(!projectVersionViewModel.IsPublished)
                {
                    await projectVersionViewModel.PublishAsync(this.workspaceManagerFactory, this.projectDataManager);                
                    logger.Information($"Version {projectVersionViewModel.Version} for project : {this.automationProject.Name} is published now.");
                }                              
                   
                this.AvailableVersions.Add(new ProjectVersionViewModel(this.automationProject, newVersion, projectFileSystem, referenceManagerFactory));
                this.wasPublished = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);              
            }           
        }

        /// <summary>
        /// Create a copy of selected version and mark the selected version as Published.
        /// </summary>
        /// <returns></returns>
        public async Task PublishAsync(ProjectVersionViewModel projectVersionViewModel)
        {
            try
            {               
                if (projectVersionViewModel.CanPublish)
                {
                    await projectVersionViewModel.PublishAsync(this.workspaceManagerFactory, this.projectDataManager);                   
                    logger.Information($"Version {projectVersionViewModel.Version} for project : {this.automationProject.Name} is published now.");
                }                              
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }


        public async Task CloseAsync()
        {
            await this.TryCloseAsync(this.wasPublished);
        }
    }
}
