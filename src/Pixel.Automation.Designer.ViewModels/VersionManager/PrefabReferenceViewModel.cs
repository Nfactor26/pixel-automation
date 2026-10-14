using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace Pixel.Automation.Designer.ViewModels.VersionManager;

/// <summary>
/// View Model wrapper for <see cref="PrefabReference"/>
/// </summary>
public class PrefabReferenceViewModel : NotifyPropertyChanged
{
    internal readonly PrefabReference prefabReference;

    /// <summary>
    /// Name of the Prefab
    /// </summary>
    public string PrefabName { get; private set; }

    /// <summary>
    /// Version of prefaab used in current version of project
    /// </summary>
    public string VersionInUse { get; private set; }

    private Version selectedVersion;
    /// <summary>
    /// Select version of prefab on view
    /// </summary>
    public Version SelectedVersion
    {
        get => this.selectedVersion;
        set
        {
            if(this.selectedVersion != value)
            {
                this.selectedVersion = value;
                this.prefabReference.Version = value;
                this.IsDirty = true;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Indicate if version was changed
    /// </summary>
    public bool IsDirty { get; private set; }

    /// <summary>
    /// Available versions of Prefab
    /// </summary>
    public ObservableCollection<Version> AvailableVersions { get; private set; } = new();

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="prefabReference"></param>
    public PrefabReferenceViewModel(IApplicationFileSystem applicationFileSystem, Prefab prefab, PrefabReference prefabReference)
    {
        Guard.Argument(prefab).NotNull();
        this.prefabReference = Guard.Argument(prefabReference).NotNull();
        this.PrefabName = prefab.DisplayName;
        this.VersionInUse = prefabReference.Version.ToString();
        foreach (var version in GetAvailableVersions(applicationFileSystem, prefab))
        {
            this.AvailableVersions.Add(version);
        }
        this.SelectedVersion = this.AvailableVersions.FirstOrDefault(a => a.Equals(prefabReference.Version))
            ?? this.AvailableVersions.First();
        this.IsDirty = false;
    }

    IEnumerable<Version> GetAvailableVersions(IApplicationFileSystem fileSystem, Prefab prefab)
    {
        string prefabsDirectory = Path.Combine(fileSystem.GetApplicationsDirectory(), prefab.ApplicationId, Constants.PrefabsDirectory, prefab.PrefabId);
        var versions = Directory.EnumerateDirectories(prefabsDirectory).Select(d => new Version(Path.GetFileName(d)));
        return versions;
    }
}
