using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    public class ProjectVersionManagerViewModel: Screen, IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<ProjectVersionManagerViewModel>();

        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly AutomationProject automationProject;
        private readonly ApplicationSettings applicationSettings;

        public BindableCollection<ProjectVersionViewModel> AvailableVersions { get; set; } = new BindableCollection<ProjectVersionViewModel>();
       
        public ProjectVersionManagerViewModel(AutomationProject automationProject, IWorkspaceManagerFactory workspaceManagerFactory,
             ISerializer serializer, IApplicationDataManager applicationDataManager, ApplicationSettings applicationSettings)
        {
            this.DisplayName = "Manage & Deploy Versions";
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.automationProject = Guard.Argument(automationProject, nameof(automationProject)).NotNull();
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            foreach (var version in this.automationProject.AvailableVersions)
            {
                IProjectFileSystem projectFileSystem = new ProjectFileSystem(serializer, applicationSettings);
                AvailableVersions.Add(new ProjectVersionViewModel(this.automationProject, version, projectFileSystem));
            }
        }

        /// <summary>
        /// Create a copy of selected version and deploy the selected version.
        /// </summary>
        /// <returns></returns>
        public async Task Deploy(ProjectVersionViewModel projectVersionViewModel)
        {
            try
            {               
                if (projectVersionViewModel?.IsActive == true)
                {
                    logger.Information($"Trying to deploy version {projectVersionViewModel} for project : {this.automationProject.Name}");
                  
                    //Create a new active version from selected version
                    ProjectVersion newVersion = projectVersionViewModel.Clone();
                    IProjectFileSystem projectFileSystem = new ProjectFileSystem(serializer, this.applicationSettings);
                    projectFileSystem.Initialize(this.automationProject.Name, newVersion);

                    //Deploy the selected version
                    projectVersionViewModel.Deploy(this.workspaceManagerFactory);                   

                    this.automationProject.AvailableVersions.Add(newVersion);
                    serializer.Serialize<AutomationProject>(projectFileSystem.ProjectFile, this.automationProject);

                    await this.applicationDataManager.AddOrUpdateProjectAsync(this.automationProject, projectVersionViewModel.ProjectVersion);
                    await this.applicationDataManager.AddOrUpdateProjectAsync(this.automationProject, newVersion);
                   
                    AvailableVersions.Add(new ProjectVersionViewModel(this.automationProject, newVersion, projectFileSystem));
                    logger.Information($"Completed deployment of version : {projectVersionViewModel} for project : {this.automationProject.Name}");
                    return;
                }              
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }           
        }
       
    }
}
