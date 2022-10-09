using Dawn;
using Newtonsoft.Json.Linq;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Persistence.Services.Client;
using Serilog;
using System.IO;

namespace Pixel.Automation.Designer.ViewModels
{
    public class NewProjectViewModel : SmartScreen, INewProject
    {
        private readonly ILogger logger = Log.ForContext<NewProjectViewModel>();
           
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly IApplicationFileSystem fileSystem;
        private List<AutomationProject> existingProjects = new List<AutomationProject>();

        public AutomationProject NewProject { get; }

        public string Name
        {
            get => this.NewProject.Name;
            set
            {
                value = value ?? string.Empty;
                this.NewProject.Name = value;               
                ValidateProjectName(value);
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => CanCreateNewProject);
            }
        }

        public NewProjectViewModel(ISerializer serializer, IApplicationDataManager applicationDataManager, IApplicationFileSystem fileSystem)
        {           
            this.DisplayName = "Create New Project";
           
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;

            Version defaultVersion = new Version(1, 0, 0, 0);
            this.NewProject = new AutomationProject();
            this.NewProject.AvailableVersions.Add(new ProjectVersion(defaultVersion) { IsActive = true, IsDeployed = false});

            this.existingProjects.AddRange(this.applicationDataManager.GetAllProjects());
        
        }    

        public  async Task CreateNewProject()
        {
            try
            {
                this.NewProject.Name = this.NewProject.Name.Trim();
                this.NewProject.Namespace = $"{Constants.NamespacePrefix}.{this.NewProject.Name.Replace(' ', '.')}";
                this.NewProject.LastOpened = DateTime.Now;

                //create a directory inside projects directory with name equal to newProject identifier
                string projectFolder = this.fileSystem.GetAutomationProjectDirectory(this.NewProject);
                if (Directory.Exists(projectFolder))
                {
                    throw new InvalidOperationException($"Project with name : {NewProject.Name} already exists");
                }
                Directory.CreateDirectory(projectFolder);

                //create and save the project file
                string projectFile = this.fileSystem.GetAutomationProjectFile(this.NewProject);
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
                return !this.HasErrors && !string.IsNullOrEmpty(this.Name) && !!IsNameAvailable(this.Name);
            }

        }

        public void ValidateProjectName(string projectName)
        {
            ClearErrors(nameof(Name));
            ValidateRequiredProperty(nameof(Name), projectName);
            ValidatePattern("^([A-Za-z]|[._ ]){4,}$", nameof(Name), projectName, "Name must contain only alphabets or ' ' or '_' and should be atleast 4 characters in length.");
            if(!IsNameAvailable(projectName))
            {
                this.AddOrAppendErrors(nameof(Name), "An application already exists with this name");
            }
            
        }

        private bool IsNameAvailable(string projectName)
        {
            return !this.existingProjects.Any(a => a.Name.Equals(projectName));            
        }

        public async void Cancel()
        {
           await this.TryCloseAsync(false);
        }

    }
}
