using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Persistence.Services.Client;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels
{
    public class NewProjectViewModel : SmartScreen, INewProject
    {
        private readonly ILogger logger = Log.ForContext<NewProjectViewModel>();

        private readonly string saveDirectory = ".\\Automations\\";  //Todo : Get this from config
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;

        public AutomationProject NewProject { get; }

        public string Name
        {
            get => this.NewProject.Name;
            set
            {
                this.NewProject.Name = value;
                ValidateRequiredProperty(nameof(Name), value);
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => CanCreateNewProject);
            }
        }

        public ProjectType ProjectType
        {
            get => this.NewProject.ProjectType;
            set => this.ProjectType = value;
        }

        public NewProjectViewModel(ISerializer serializer, IApplicationDataManager applicationDataManager)
        {
            Guard.Argument(serializer).NotNull($"{nameof(serializer)} is required parameter");
            Guard.Argument(applicationDataManager).NotNull($"{nameof(applicationDataManager)} is required parameter");

            this.DisplayName = "Create New Project";
            this.serializer = serializer;
            this.applicationDataManager = applicationDataManager;
            Version defaultVersion = new Version(1, 0, 0, 0);
            this.NewProject = new AutomationProject()
            {
                ProjectId = Guid.NewGuid().ToString(), 
                LastOpened = DateTime.Now,
                ProjectType = ProjectType.TestAutomation
            };
            this.NewProject.AvailableVersions.Add(new ProjectVersion(defaultVersion) { IsActive = true, IsDeployed = false});
        
        }    

        public  async Task CreateNewProject()
        {
            this.NewProject.LastOpened = DateTime.Now;

            //create a directory inside Automations directory with name equal to newProject name
            string projectFolder = Path.Combine(saveDirectory, this.NewProject.Name);
            if (Directory.Exists(projectFolder))
            {
                throw new InvalidOperationException($"Project with name : {NewProject.Name} already exists");
            }
            Directory.CreateDirectory(projectFolder);            

            //create and save the project file
            string projectFile = Path.Combine(projectFolder, this.NewProject.Name + ".atm");
            serializer.Serialize<AutomationProject>(projectFile, this.NewProject, null);
            await this.applicationDataManager.AddOrUpdateProjectAsync(this.NewProject, null);
            logger.Information($"Created new project : {this.Name} of type : {this.ProjectType}");

            await this.TryCloseAsync(true);
        }

        public bool CanCreateNewProject
        {
            get
            {
                return !this.HasErrors && !string.IsNullOrEmpty(this.Name);
            }

        }

        public async void Cancel()
        {
           await this.TryCloseAsync(false);
        }

    }
}
