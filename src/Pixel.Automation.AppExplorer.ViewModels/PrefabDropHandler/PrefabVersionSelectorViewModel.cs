using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.ViewModels;
using Pixel.Automation.Reference.Manager.Contracts;
using Serilog;
using System.Windows;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabDropHandler;

/// <summary>
/// Show available versions of the Prefab and allow user to select version to use.
/// If a version has been previously used in automation process, change of version won't be allowed.  
/// </summary>
public class PrefabVersionSelectorViewModel : StagedSmartScreen
{
    private readonly ILogger logger = Log.ForContext<PrefabVersionSelectorViewModel>();
  
    private readonly PrefabProject prefabProject;
    private readonly PrefabHostEntity prefabEntity;
    private readonly EntityComponentViewModel dropTarget;       
 
    private readonly IReferenceManager projectReferenceManager;
    
    /// <summary>
    /// Name of the Prefab
    /// </summary>
    public string PrefabName => prefabProject.Name;

    /// <summary>
    /// Available versions of the Prefab
    /// </summary>
    public IEnumerable<VersionInfo> AvailableVersions { get; private set; }

    /// <summary>
    /// Select version of Prefab to use
    /// </summary>
    public VersionInfo SelectedVersion { get; set; }

    /// <summary>
    /// Indicates if the version can be changed.
    /// Version change is not allowed if the process is already using specific version of this prefab.
    /// </summary>
    public bool CanChangeVersion { get; private set; }
   
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="projectFileSystem">File system associated with the automation project to which Prefab is added</param>
    /// <param name="prefabFileSystem">File System for the Prefab that is being added</param>
    /// <param name="prefabEntity">PrefabEntity that is being added</param>
    /// <param name="prefabProjectViewModel">Prefab which needs to be added to automation process</param>
    /// <param name="dropTarget">EntityComponentViewModel wrapping an Entity to which Prefab needs to be added</param>
    public PrefabVersionSelectorViewModel(IReferenceManager projectReferenceManager, PrefabHostEntity prefabEntity,
        PrefabProjectViewModel prefabProjectViewModel, EntityComponentViewModel dropTarget)
    {
        this.DisplayName = "Select prefab version";            
        this.projectReferenceManager = projectReferenceManager;          
        this.prefabEntity = prefabEntity;
        this.prefabProject = prefabProjectViewModel.PrefabProject;
        this.dropTarget = dropTarget;          
        this.AvailableVersions = prefabProject.AvailableVersions;
        this.CanChangeVersion = !projectReferenceManager.GetPrefabReferences().HasReference(prefabProject);
        if(!this.CanChangeVersion)
        {
            var prefabReference = projectReferenceManager.GetPrefabReferences().GetPrefabReference(prefabProject.ProjectId);             
            this.SelectedVersion = AvailableVersions.First(a => a.Equals(prefabReference.Version));              
        }
        else
        {
            this.SelectedVersion = AvailableVersions.Last();              
        }         
    }
    
    /// <inheritdoc/>   
    public override async Task<bool> TryProcessStage()
    {
        try
        {
            if (this.SelectedVersion != null && !dropTarget.ComponentCollection.Any(a => a.Model.Equals(prefabEntity)))
            {
                await UpdatePrefabReferencesAsync();                 
                dropTarget.AddComponent(prefabEntity);
                this.CanChangeVersion = false;
                logger.Information("Added version {0} of {1} to {2}.", this.SelectedVersion, this.prefabProject, this.dropTarget);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "There was an error while trying to process stage for the prefab version selector screen");
            MessageBox.Show("Error while trying to add prefab", "Add Prefab Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }  
    }

    /// <summary>
    /// When adding a Prefab, make an entry of the Prefab in to the Prefab References file of the automation Process if it doesn't already exist.
    /// </summary>
    private async Task UpdatePrefabReferencesAsync()
    {
        await this.projectReferenceManager.AddPrefabReferenceAsync(new PrefabReference() 
        { 
            ApplicationId = prefabProject.ApplicationId,
            PrefabId = prefabProject.ProjectId, 
            Version = this.SelectedVersion
        });
        logger.Debug($"Updated prefab refrences file.");
    }
    
    /// <inheritdoc/>
    public override object GetProcessedResult()
    {
        return this.SelectedVersion;
    }

    public override bool Validate()
    {
        ClearErrors("");                  
        return base.Validate();
    }
}
