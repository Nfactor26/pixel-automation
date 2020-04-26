using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;

namespace Pixel.Automation.Designer.ViewModels
{
    public class NewProjectViewModel : Screen, INewProject
    {

        ISerializer serializer;       

        string saveDirectory = ".\\Automations\\";  //Todo : Get this from config


        AutomationProject newProject;
        public AutomationProject NewProject
        {
            get
            {
                return newProject;
            }

            set
            {
                newProject = value;
                NotifyOfPropertyChange(() => NewProject);
            }
        }

        public NewProjectViewModel(ISerializer serializer)
        {
            Guard.Argument<ISerializer>(serializer).NotNull($"{nameof(serializer)} is reuired parameter");

            this.DisplayName = "New Project";
            this.serializer = serializer;
            Version defaultVersion = new Version(1, 0, 0, 0);
            this.NewProject = new AutomationProject()
            {
                ProjectId = Guid.NewGuid().ToString(), 
                LastOpened = DateTime.Now
            };
            this.NewProject.AvailableVersions.Add(new ProjectVersion(defaultVersion) { IsActive = true, IsDeployed = false});
            this.NewProject.PropertyChanged += NewProject_PropertyChanged;
        }

        private void NewProject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => CanCreateNewProject);
        }

        public  async void CreateNewProject()
        {
            this.newProject.LastOpened = DateTime.Now;

            //create a directory inside Automations directory with name equal to newProject name
            string projectFolder = Path.Combine(saveDirectory, this.newProject.Name);
            if (Directory.Exists(projectFolder))
            {
                throw new InvalidOperationException($"Project with name : {newProject.Name} already exists");
            }
            Directory.CreateDirectory(projectFolder);            

            //create and save the project file
            string projectFile = Path.Combine(projectFolder, this.newProject.Name + ".atm");
            serializer.Serialize<AutomationProject>(projectFile, this.newProject, null);

            await this.TryCloseAsync(true);
        }

        public bool CanCreateNewProject
        {
            get
            {
                return !this.newProject.HasErrors && !string.IsNullOrEmpty(NewProject.Name);
            }

        }

        public async void Cancel()
        {
           await this.TryCloseAsync(false);
        }

    }
}
