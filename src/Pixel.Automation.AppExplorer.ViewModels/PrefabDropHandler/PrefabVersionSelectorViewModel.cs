using Microsoft.Win32;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.ViewModels;
using Pixel.Scripting.Reference.Manager;
using Pixel.Scripting.Reference.Manager.Contracts;
using Serilog;
using System.IO;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabDropHandler
{
    /// <summary>
    /// Show available versions of the Prefab and allow user to select version to use.
    /// If a version has been previously used in automation process, change of version won't be allowed.  
    /// </summary>
    public class PrefabVersionSelectorViewModel : StagedSmartScreen
    {
        private readonly ILogger logger = Log.ForContext<PrefabVersionSelectorViewModel>();
      
        private readonly PrefabProject prefabProject;
        private readonly PrefabEntity prefabEntity;
        private readonly EntityComponentViewModel dropTarget;
       
        private readonly IProjectFileSystem projectFileSystem;
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly IReferenceManager projectReferenceManager;
        
        /// <summary>
        /// Name of the Prefab
        /// </summary>
        public string PrefabName => prefabProject.PrefabName;

        /// <summary>
        /// Available versions of the Prefab
        /// </summary>
        public IEnumerable<PrefabVersion> AvailableVersions { get; private set; }

        /// <summary>
        /// Select version of Prefab to use
        /// </summary>
        public PrefabVersion SelectedVersion { get; set; }

        /// <summary>
        /// Indicates if the version can be changed.
        /// Version change is not allowed if the process is already using specific version of this prefab.
        /// </summary>
        public bool CanChangeVersion { get; private set; }

        /// <summary>
        /// Input mapping script file for the prefab
        /// </summary>
        public string InputMappingScriptFile
        {
            get => prefabEntity.InputMappingScriptFile;
            set 
            {
                prefabEntity.InputMappingScriptFile = value;            
                Validate();
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Output mapping script file for the prefab
        /// </summary>
        public string OutputMappingScriptFile
        {
            get => prefabEntity.OutputMappingScriptFile;
            set
            {
                prefabEntity.OutputMappingScriptFile = value;              
                Validate();
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="projectFileSystem">File system associated with the automation project to which Prefab is added</param>
        /// <param name="prefabFileSystem">File System for the Prefab that is being added</param>
        /// <param name="prefabEntity">PrefabEntity that is being added</param>
        /// <param name="prefabProjectViewModel">Prefab which needs to be added to automation process</param>
        /// <param name="dropTarget">EntityComponentViewModel wrapping an Entity to which Prefab needs to be added</param>
        public PrefabVersionSelectorViewModel(IProjectFileSystem projectFileSystem, IPrefabFileSystem prefabFileSystem,
            IReferenceManager projectReferenceManager,  PrefabEntity prefabEntity,
            PrefabProjectViewModel prefabProjectViewModel, EntityComponentViewModel dropTarget)
        {
            this.DisplayName = "(1/3) Select prefab version and mapping scripts";
            this.projectFileSystem = projectFileSystem;
            this.prefabFileSystem = prefabFileSystem;
            this.projectReferenceManager = projectReferenceManager;
            this.prefabEntity = prefabEntity;
            this.prefabProject = prefabProjectViewModel.PrefabProject;
            this.dropTarget = dropTarget;          
            this.AvailableVersions = prefabProject.PublishedVersions;
            this.CanChangeVersion = !projectReferenceManager.GetPrefabReferences().HasReference(prefabProject);
            if(!this.CanChangeVersion)
            {
                var referencedVersion = projectReferenceManager.GetPrefabReferences().GetPrefabVersionInUse(prefabProject);
                this.SelectedVersion = AvailableVersions.First(a => a.Equals(referencedVersion));
            }
            else
            {
                this.SelectedVersion = AvailableVersions.Last();
            }           
        }

        /// <summary>
        /// Pick an existing input mapping script file
        /// </summary>
        public void PickInputMappingScriptFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSX File (*.csx)|*.csx";
            openFileDialog.InitialDirectory = projectFileSystem.WorkingDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                this.InputMappingScriptFile = Path.GetRelativePath(projectFileSystem.WorkingDirectory, openFileDialog.FileName);
                logger.Information("Input mapping script file changed to {0} for Prefab", this.InputMappingScriptFile);               
            }
        }

        /// <summary>
        /// Pick an existing output mapping script file
        /// </summary>
        public void PickOutputMappingScriptFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSX File (*.csx)|*.csx";
            openFileDialog.InitialDirectory = projectFileSystem.WorkingDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                this.OutputMappingScriptFile = Path.GetRelativePath(projectFileSystem.WorkingDirectory, openFileDialog.FileName);
                logger.Information("Output mapping script file changed to {0} for Prefab", this.OutputMappingScriptFile);
            }
        }

        /// <inheritdoc/>   
        public override bool TryProcessStage(out string errorDescription)
        {
            //TODO : Can we make this asyc ?
            if(this.SelectedVersion != null && !dropTarget.ComponentCollection.Any(a => a.Model.Equals(prefabEntity)))
            {
                UpdatePrefabReferences();
                UpdateControlReferences();
                dropTarget.AddComponent(prefabEntity);
                this.CanChangeVersion = false;
                logger.Information("Added version {0} of {1} to {2}.", this.SelectedVersion, this.prefabProject, this.dropTarget);
            }
            errorDescription = String.Empty;
            return true;        
        }

        /// <summary>
        /// When adding a Prefab, make an entry of the Prefab in to the Prefab References file of the automation Process if it doesn't already exist.
        /// </summary>
        private void UpdatePrefabReferences()
        {
            _ = this.projectReferenceManager.AddPrefabReferenceAsync(new PrefabReference() { ApplicationId = prefabProject.ApplicationId, PrefabId = prefabProject.PrefabId, Version = this.SelectedVersion });
            logger.Debug($"Updated prefab refrences file.");
        }

        /// <summary>
        /// A Prefab might use one or more control. When adding a Prefab to an automation process, we need to update the Control References file
        /// of the automation process to include details of controls being used by the Prefab.
        /// </summary>
        private void UpdateControlReferences()
        {
            //Load control references file for prefab project
            this.prefabFileSystem.Initialize(this.prefabProject, this.SelectedVersion);
            ProjectReferences prefabProjectReferences = File.Exists(this.prefabFileSystem.ReferencesFile) ?
                this.prefabFileSystem.LoadFile<ProjectReferences>(this.prefabFileSystem.ReferencesFile) : new ProjectReferences();

            //Load control references file for project to which prefab is being added
            ControlReferences projectControlReferences = this.projectReferenceManager.GetControlReferences();

            //Add control references from prefab project to automation project if it doesn't already exists and save it
            //TODO : Batch all the additions
            foreach (var controlReference in prefabProjectReferences.ControlReferences)
            {
                if (!projectControlReferences.HasReference(controlReference.ControlId))
                {
                    _ = this.projectReferenceManager.AddControlReferenceAsync(controlReference);
                }
            }           
            logger.Debug($"Updated control refrences file.");
        }

        /// <inheritdoc/>
        public override object GetProcessedResult()
        {
            return this.SelectedVersion;
        }

        public override bool Validate()
        {
            ClearErrors("");
            bool isValid;
            isValid = ValidateScriptPath(nameof(this.InputMappingScriptFile), this.InputMappingScriptFile);
            isValid = isValid && ValidateScriptPath(nameof(this.OutputMappingScriptFile), this.OutputMappingScriptFile);           
            return isValid && base.Validate();
        }

        private bool ValidateScriptPath(string propertyName, string scriptFile)
        {
            ClearErrors(propertyName);
            string scriptDirectory = Path.GetDirectoryName(Path.Combine(this.projectFileSystem.WorkingDirectory, scriptFile));
            if (!(scriptDirectory.Contains(this.projectFileSystem.ScriptsDirectory) || scriptDirectory.Contains(this.projectFileSystem.TestCaseRepository)))
            {
                AddOrAppendErrors(propertyName, $"{propertyName} file must be located in a subdirectory of '{this.projectFileSystem.WorkingDirectory}' directory");
                AddOrAppendErrors("", $"{propertyName} file must be located in a subdirectory of '{this.projectFileSystem.WorkingDirectory}' directory");               
                return false;
            }
            return true;
        }
    }
}
