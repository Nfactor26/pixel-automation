using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Reference.Manager.Contracts;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefabs;

/// <summary>
/// Used to track Prefabs that are open for edit
/// </summary>
internal class PrefabInstance
{
    public string PrefabId => Prefab.PrefabId;

    public Prefab Prefab { get; init; }

    public VersionInfo Version { get; init; }

    public Entity PrefabEntity { get; set; }

    public IReferenceManager ReferenceManager { get; set; } 
}
