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
        private readonly PrefabDescription prefabDescription;
        private readonly PrefabReferences prefabReferences;
        private readonly IProjectFileSystem projectFileSystem;

        public string PrefabName => prefabDescription.PrefabName;

        public IEnumerable<PrefabVersion> AvailableVersions { get; private set; }

        public PrefabVersion SelectedVersion { get; set; }

        public bool CanChangeVersion { get; private set; } 

        public PrefabVersionSelectorViewModel(IProjectFileSystem projectFileSystem, PrefabDescription prefabDescription, string entityId, string testCaseId)
        {          
            this.projectFileSystem = projectFileSystem;
            this.prefabDescription = prefabDescription;
            if(File.Exists(projectFileSystem.PrefabReferencesFile))
            {
                this.prefabReferences = projectFileSystem.LoadFile<PrefabReferences>(projectFileSystem.PrefabReferencesFile);
            }
            else
            {
                this.prefabReferences = new PrefabReferences();
            }
            this.AvailableVersions = prefabDescription.DeployedVersions;
            this.CanChangeVersion = !prefabReferences.HasReference(prefabDescription);
            if(!this.CanChangeVersion)
            {
                var referencedVersion = prefabReferences.GetPrefabVersionInUse(prefabDescription);
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
            if(!prefabReferences.HasReference(prefabDescription))
            {                
                prefabReferences.AddPrefabReference(new PrefabReference() { ApplicationId = prefabDescription.ApplicationId, PrefabId = prefabDescription.PrefabId, Version = this.SelectedVersion });
            }            
            this.projectFileSystem.SaveToFile<PrefabReferences>(prefabReferences, this.projectFileSystem.WorkingDirectory, Path.GetFileName(this.projectFileSystem.PrefabReferencesFile));
            await this.TryCloseAsync(true);
            logger.Information($"{nameof(PrefabVersionSelectorViewModel)} was closed by clicking ok. {this.SelectedVersion} was selected.");

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
