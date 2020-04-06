using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
            this.NewProject.AvailableVersions.Add(new ProjectVersion(defaultVersion));
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
            //Directory.CreateDirectory(Path.Combine(projectFolder, "Resources"));
            //create a directory with name CustomComponents inside the project folder
            //string customComponentsFolder = Path.Combine(projectFolder, "CustomComponents");
            //Directory.CreateDirectory(customComponentsFolder);

            //copy the visual studio sln files from template folder to custom components folder           
            //string sourceFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Template";
            //string destinationFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("Automations", this.NewProject.Name, "CustomComponents"));
            //foreach (string dirPath in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories))
            //    Directory.CreateDirectory(dirPath.Replace(sourceFolder, destinationFolder));
            //foreach (string newPath in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories))
            //    File.Copy(newPath, newPath.Replace(sourceFolder, destinationFolder), true);

            //Add the default assembly references to the visual studio project

            //AddDefaultReferencesToProject(this.newProject.GeneratedSlnPath);

            //create and save the project file
            string projectFile = Path.Combine(projectFolder, this.newProject.Name + ".atm");
            serializer.Serialize<AutomationProject>(projectFile, this.newProject, null);

            await this.TryCloseAsync(true);
        }


        //private void AddDefaultReferencesToProject(string targetSln)
        //{
        //    if (File.Exists("DefaultReferences.dat"))
        //    {
        //        string data = File.ReadAllText("DefaultReferences.dat");
        //        List<string> references = data.Split(new char[] { ',' }).Select(n => n.Trim().Trim(new char[] { '"', '\\' })).ToList<string>();
        //        compiler.CreateWorkSpace(targetSln, string.Empty);
        //        compiler.AddReferences(references);
        //    }

        //}

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
