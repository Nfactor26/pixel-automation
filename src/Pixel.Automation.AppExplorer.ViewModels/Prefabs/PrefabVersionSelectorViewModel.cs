using Caliburn.Micro;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.ViewModels;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Automation.Core;
using Serilog;
using Pixel.Automation.Core.Interfaces;
using System.IO;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefabs;

/// <summary>
/// Show available versions of the Prefab and allow user to select version to use.
/// If a version has been previously used in automation process, change of version won't be allowed.  
/// </summary>
public class PrefabVersionSelectorViewModel : SmartScreen
{
    private readonly ILogger logger = Log.ForContext<PrefabVersionSelectorViewModel>();
  
    private readonly Prefab prefab;   
    private readonly EntityComponentViewModel targetItem;

    private readonly IReferenceManager projectReferenceManager;
    private readonly IFileSystem fileSystem;
    private readonly IEventAggregator eventAggregator;
    
    /// <summary>
    /// Name of the Prefab
    /// </summary>
    public string PrefabName => prefab.DisplayName;

    /// <summary>
    /// Available versions of the Prefab
    /// </summary>
    public IEnumerable<Version> AvailableVersions { get; private set; }

    /// <summary>
    /// Select version of Prefab to use
    /// </summary>
    public Version SelectedVersion { get; set; }

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
    public PrefabVersionSelectorViewModel(IReferenceManager projectReferenceManager,
        IFileSystem fileSystem, IEventAggregator eventAggregator,
        PrefabViewModel prefabViewModel, EntityComponentViewModel dropTarget)
    {
        this.DisplayName = "Select prefab version to use";            
        this.projectReferenceManager = projectReferenceManager;
        this.fileSystem = fileSystem;
        this.eventAggregator = eventAggregator;       
        this.prefab = prefabViewModel.Prefab;
        this.targetItem = dropTarget;          
        this.AvailableVersions = GetAvailableVersions(this.prefab);
        this.CanChangeVersion = !projectReferenceManager.GetPrefabReferences().HasReference(prefab);
        if(!this.CanChangeVersion)
        {
            var prefabReference = projectReferenceManager.GetPrefabReferences().GetPrefabReference(prefab.PrefabId);             
            this.SelectedVersion = AvailableVersions.First(a => a.Equals(prefabReference.Version));             
        }
        else
        {
            this.SelectedVersion = AvailableVersions.Last();              
        }           
    }

    IEnumerable<Version> GetAvailableVersions(Prefab prefab)
    {
        string prefabsDirectory = Path.Combine(fileSystem.ApplicationsDirectory, prefab.ApplicationId, Constants.PrefabsDirectory, prefab.PrefabId);
        var versions = Directory.EnumerateDirectories(prefabsDirectory).Select(d =>  new Version(Path.GetFileName(d)));
        return versions;
    }


    /// <summary>
    /// Apply the edits done of temporary copy to the actual instance of TestCase
    /// and close the screen.
    /// </summary>
    /// <returns></returns>
    public async Task Save()
    {
        try
        {
            if (Validate())
            {
                targetItem.AddComponent(new PrefabHostEntity() { ApplicationId = prefab.ApplicationId, PrefabId = prefab.PrefabId, Name = prefab.DisplayName });

                if (targetItem.Model.TryGetAnsecstorOfType(out TestCaseEntity testCaseEntity))
                {
                    targetItem.Model.TryGetAnsecstorOfType(out TestFixtureEntity testFixtureEntity);
                    _ = this.eventAggregator.PublishOnBackgroundThreadAsync(new PrefabAddedEventArgs(prefab.PrefabId, testFixtureEntity.Tag, testCaseEntity.Tag));
                }
                else if (targetItem.Model.TryGetAnsecstorOfType(out TestFixtureEntity testFixtureEntity))
                {
                    _ = this.eventAggregator.PublishOnBackgroundThreadAsync(new PrefabAddedEventArgs(prefab.PrefabId, testFixtureEntity.Tag));
                }

                await UpdatePrefabReferencesAsync();
                await this.TryCloseAsync(true);
                
                logger.Information("Version {0} of Prefab {1} was added", this.SelectedVersion, this.prefab.PrefabId);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, ex.Message);
        }
    }

    /// <summary>
    /// When adding a Prefab, make an entry of the Prefab in to the Prefab References file of the automation Process if it doesn't already exist.
    /// </summary>
    private async Task UpdatePrefabReferencesAsync()
    {
        await this.projectReferenceManager.AddPrefabReferenceAsync(new PrefabReference()
        {
            ApplicationId = prefab.ApplicationId,
            PrefabId = prefab.PrefabId,
            Version = this.SelectedVersion
        });
        logger.Debug($"Updated prefab refrences file.");
    }


    /// <summary>
    /// Close the screen without applying any edits done.
    /// </summary>
    /// <returns></returns>
    public async Task Cancel()
    {
        await this.TryCloseAsync(false);
        logger.Information("Edit Prefab changes were cancelled.");
    }

    #region Validation

    /// <inheritdoc/>
    public override bool ShowModelErrors => HasErrors;

    /// <inheritdoc/>
    public override void DismissModelErrors()
    {
        NotifyOfPropertyChange(() => ShowModelErrors);
    }

    private bool Validate()
    {
        ValidateProperty(nameof(SelectedVersion));
        return !HasErrors;
    }

    private void ValidateProperty(string propertyName)
    {
        ClearErrors(propertyName);
        switch (propertyName)
        {
            case nameof(SelectedVersion):
                if (this.SelectedVersion is null)
                {
                    AddOrAppendErrors(propertyName, "Version is required");
                }              
                break;
        }
    }

    #endregion Validation
  
}
