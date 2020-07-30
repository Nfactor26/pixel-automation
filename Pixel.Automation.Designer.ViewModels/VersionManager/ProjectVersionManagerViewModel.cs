using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    public class ProjectVersionManagerViewModel: Screen
    {
        private readonly ILogger logger = Log.ForContext<ProjectVersionManagerViewModel>();

        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly AutomationProject automationProject;

        public BindableCollection<ProjectVersionViewModel> AvailableVersions { get; set; } = new BindableCollection<ProjectVersionViewModel>();

        private ProjectVersionViewModel selectedVersion;
        public ProjectVersionViewModel SelectedVersion
        {
            get => this.selectedVersion;
            set
            {
                this.selectedVersion = value;
                NotifyOfPropertyChange(() => CanDeploy);
            }
        }
       
        public ProjectVersionManagerViewModel(AutomationProject automationProject, IWorkspaceManagerFactory workspaceManagerFactory, 
            IApplicationDataManager applicationDataManager,
            ISerializer serializer)
        {
            this.DisplayName = "Manage & Deploy Versions";
            this.workspaceManagerFactory = workspaceManagerFactory;
            this.serializer = serializer;
            this.applicationDataManager = applicationDataManager;
            this.automationProject = automationProject;
            foreach (var version in this.automationProject.AvailableVersions)
            {
                IProjectFileSystem projectFileSystem = new ProjectFileSystem(serializer);
                AvailableVersions.Add(new ProjectVersionViewModel(this.automationProject, version, projectFileSystem));
            }
        }

        public bool CanDeploy
        {
            get
            {
                return this.SelectedVersion?.IsActive ?? false;
            }

        }

        /// <summary>
        /// Create a copy of selected version and deploy the selected version.
        /// </summary>
        /// <returns></returns>
        public async Task Deploy()
        {
            try
            {
                if (this.SelectedVersion?.IsActive == true)
                {
                    //Create a new active version from selected version
                    ProjectVersion newVersion = this.SelectedVersion.Clone();

                    IProjectFileSystem projectFileSystem = new ProjectFileSystem(serializer);
                    projectFileSystem.Initialize(this.automationProject.Name, newVersion);
                    AvailableVersions.Add(new ProjectVersionViewModel(this.automationProject, newVersion, projectFileSystem));

                    //Deploy the selected version
                    this.SelectedVersion.Deploy(this.workspaceManagerFactory);

                    this.automationProject.AvailableVersions.Add(newVersion);
                    serializer.Serialize<AutomationProject>(projectFileSystem.ProjectFile, this.automationProject);
                    await this.applicationDataManager.AddOrUpdateProjectAsync(this.automationProject, this.SelectedVersion.ProjectVersion);
                    await this.applicationDataManager.AddOrUpdateProjectAsync(this.automationProject, newVersion);
                }             
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }           
        }
       
    }
}
