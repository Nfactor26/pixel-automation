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
           
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;

        public AutomationProject NewProject { get; }

        public string Name
        {
            get => this.NewProject.Name;
            set
            {
                this.NewProject.Name = value;
                ValidateProjectName(value);
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => CanCreateNewProject);
            }
        }

        public NewProjectViewModel(ISerializer serializer, IApplicationDataManager applicationDataManager)
        {           
            this.DisplayName = "Create New Project";
           
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;

            Version defaultVersion = new Version(1, 0, 0, 0);
            this.NewProject = new AutomationProject();
            this.NewProject.AvailableVersions.Add(new ProjectVersion(defaultVersion) { IsActive = true, IsDeployed = false});
        
        }    

        public  async Task CreateNewProject()
        {
            try
            {
                this.NewProject.LastOpened = DateTime.Now;

                //create a directory inside projects directory with name equal to newProject name
                string projectFolder = this.applicationDataManager.GetProjectDirectory(this.NewProject);
                if (Directory.Exists(projectFolder))
                {
                    throw new InvalidOperationException($"Project with name : {NewProject.Name} already exists");
                }
                Directory.CreateDirectory(projectFolder);

                //create and save the project file
                string projectFile = this.applicationDataManager.GetProjectFile(this.NewProject);
                serializer.Serialize<AutomationProject>(projectFile, this.NewProject, null);
                await this.applicationDataManager.AddOrUpdateProjectAsync(this.NewProject, null);
                logger.Information($"Created new project : {this.Name}");

                await this.TryCloseAsync(true);
            }
            catch (Exception ex)
            {
                logger.Warning($"There was an error while trying to create new project : {this.NewProject.Name}");
                logger.Error(ex.Message, ex);
                await this.TryCloseAsync(false);
            }
        }

        public bool CanCreateNewProject
        {
            get
            {
                return !this.HasErrors && !string.IsNullOrEmpty(this.Name);
            }

        }

        public void ValidateProjectName(string projectName)
        {
            ClearErrors(nameof(Name));
            ValidateRequiredProperty(nameof(Name), projectName);
            ValidatePattern("^([A-Za-z]|[_]){4,}$", nameof(Name), projectName, "Name must contain only alphabets or '_' and should be atleast 4 characters in length.");
        }

        public async void Cancel()
        {
           await this.TryCloseAsync(false);
        }

    }
}
