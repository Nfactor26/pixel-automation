using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System.Diagnostics;
using System.IO;

namespace Pixel.Automation.Designer.ViewModels
{
    public class NewProjectViewModel : SmartScreen, INewProject
    {
        private readonly ILogger logger = Log.ForContext<NewProjectViewModel>();
           
        private readonly ISerializer serializer;
        private readonly IProjectDataManager projectDataManager;
        private readonly IApplicationFileSystem applicationFileSystem;
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

        public NewProjectViewModel(ISerializer serializer, IProjectDataManager projectDataManager, IApplicationFileSystem applicationFileSystem)
        {           
            this.DisplayName = "Create New Project";
           
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.projectDataManager = Guard.Argument(projectDataManager, nameof(projectDataManager)).NotNull().Value;
            this.applicationFileSystem = Guard.Argument(applicationFileSystem).NotNull().Value;

            Version defaultVersion = new Version(1, 0, 0, 0);
            this.NewProject = new AutomationProject();
            this.NewProject.AvailableVersions.Add(new ProjectVersion(defaultVersion) { IsActive = true});

            this.existingProjects.AddRange(this.projectDataManager.GetAllProjects());
        
        }    

        public  async Task CreateNewProject()
        {
            using (var activity = Telemetry.DefaultSource.StartActivity(nameof(CreateNewProject), ActivityKind.Internal))
            {
                try
                {
                    this.NewProject.Name = this.NewProject.Name.Trim();
                    this.NewProject.Namespace = $"{Constants.NamespacePrefix}.{this.NewProject.Name.Replace(' ', '.')}";

                    activity?.SetTag("ProjectName", this.NewProject.Name);
                    activity?.SetTag("Namespace", this.NewProject.Namespace);

                    //create a directory inside projects directory with name equal to newProject identifier
                    string projectFolder = this.applicationFileSystem.GetAutomationProjectDirectory(this.NewProject);
                    if (Directory.Exists(projectFolder))
                    {
                        throw new InvalidOperationException($"Project with name : {NewProject.Name} already exists");
                    }
                    Directory.CreateDirectory(projectFolder);

                    await this.projectDataManager.AddProjectAsync(this.NewProject);

                    //create and save the project file
                    string projectFile = this.applicationFileSystem.GetAutomationProjectFile(this.NewProject);
                    serializer.Serialize<AutomationProject>(projectFile, this.NewProject, null);

                    logger.Information("Created new project : {0}", this.Name);

                    await this.TryCloseAsync(true);
                }
                catch (Exception ex)
                {
                    logger.Warning("There was an error while trying to create new project : {0}", this.NewProject.Name);
                    logger.Error(ex.Message, ex);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await this.TryCloseAsync(false);
                }
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
            ValidatePattern("^([A-Za-z]|[_ ]){4,}$", nameof(Name), projectName, "Name must contain only alphabets or ' ' or '_' and should be atleast 4 characters in length.");
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
