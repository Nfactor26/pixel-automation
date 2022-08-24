using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels
{
    public class PrefabVersionSelectorViewModel : ScreenBase
    {
        private readonly ILogger logger = Log.ForContext<PrefabVersionSelectorViewModel>();

        private readonly string entityId;
        private readonly string testCaseId;
        private readonly PrefabProject prefabProject;
        private readonly PrefabReferences prefabReferences;
        private readonly IProjectFileSystem projectFileSystem;
        private readonly IPrefabFileSystem prefabFileSystem;

        public string PrefabName => prefabProject.PrefabName;

        public IEnumerable<PrefabVersion> AvailableVersions { get; private set; }

        public PrefabVersion SelectedVersion { get; set; }

        public bool CanChangeVersion { get; private set; } 

        public PrefabVersionSelectorViewModel(IProjectFileSystem projectFileSystem, IPrefabFileSystem prefabFileSystem, PrefabProject prefabProject, string entityId, string testCaseId)
        {          
            this.projectFileSystem = projectFileSystem;
            this.prefabFileSystem = prefabFileSystem;          
            this.prefabProject = prefabProject;
            if(File.Exists(projectFileSystem.PrefabReferencesFile))
            {
                this.prefabReferences = projectFileSystem.LoadFile<PrefabReferences>(projectFileSystem.PrefabReferencesFile);
            }
            else
            {
                this.prefabReferences = new PrefabReferences();
            }
            this.AvailableVersions = prefabProject.DeployedVersions;
            this.CanChangeVersion = !prefabReferences.HasReference(prefabProject);
            if(!this.CanChangeVersion)
            {
                var referencedVersion = prefabReferences.GetPrefabVersionInUse(prefabProject);
                this.SelectedVersion = AvailableVersions.First(a => a.Equals(referencedVersion));
            }
            else
            {
                this.SelectedVersion = AvailableVersions.Last();
            }
            this.entityId = entityId;
            this.testCaseId = testCaseId;
        }

        public async Task ConfirmSelection()
        {
            UpdatePrefabReferences();
            UpdateControlReferences();
            await this.TryCloseAsync(true);
            logger.Information($"{nameof(PrefabVersionSelectorViewModel)} was closed by clicking ok. {this.SelectedVersion} was selected.");

        }

        private void UpdatePrefabReferences()
        {
            if (!prefabReferences.HasReference(prefabProject))
            {
                prefabReferences.AddPrefabReference(new PrefabReference() { ApplicationId = prefabProject.ApplicationId, PrefabId = prefabProject.PrefabId, Version = this.SelectedVersion });
            }
            this.projectFileSystem.SaveToFile<PrefabReferences>(prefabReferences, this.projectFileSystem.WorkingDirectory, Path.GetFileName(this.projectFileSystem.PrefabReferencesFile));
            logger.Debug($"Updated prefab refrences file.");
        }

        private void UpdateControlReferences()
        {
            //Load control references file for prefab project
            this.prefabFileSystem.Initialize(this.prefabProject, this.SelectedVersion);
            ControlReferences prefabControlReferences = File.Exists(this.prefabFileSystem.ControlReferencesFile) ? 
                this.prefabFileSystem.LoadFile<ControlReferences>(this.prefabFileSystem.ControlReferencesFile) : new ControlReferences();      
           
            //Load control references file for project to which prefab is being added
            ControlReferences projectControlReferences = File.Exists(this.projectFileSystem.ControlReferencesFile) ?
                this.projectFileSystem.LoadFile<ControlReferences>(this.projectFileSystem.ControlReferencesFile) : new ControlReferences();
         
            //Add control references from prefab project to automation project if it doesn't already exists and save it
            foreach (var controlReference in prefabControlReferences.References)
            {
                if (!projectControlReferences.HasReference(controlReference.ControlId))
                {
                    projectControlReferences.AddControlReference(controlReference);
                }
            }          
            this.projectFileSystem.SaveToFile<ControlReferences>(projectControlReferences, Path.GetDirectoryName(this.projectFileSystem.ControlReferencesFile), Path.GetFileName(this.projectFileSystem.ControlReferencesFile));
           
            logger.Debug($"Updated control refrences file.");
        }

        public override bool CanClose()
        {
            return true;
        }

        public override async void CloseScreen()
        {
            await this.TryCloseAsync(false);
            logger.Information($"{nameof(PrefabVersionSelectorViewModel)} was closed by clicking cancel.");
        }
    }
}
