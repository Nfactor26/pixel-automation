using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Pixel.Automation.PrefabExplorer.ViewModels;

/// <summary>
/// Host control for the TestDataExplorer screen
/// </summary>
public class PrefabExplorerHostViewModel : AnchorableHost, IPrefabExplorerHost
{
    /// <summary>
    /// Preferred location of the panel
    /// </summary>
    public override PaneLocation PreferredLocation
    {
        get { return PaneLocation.Bottom; }
    }

    private IScreen defaultContent = new MockPrefabExplorerViewModel();

    public PrefabExplorerHostViewModel()
    {
        this.DisplayName = "Prefabs Explorer";
        _ = this.ActivateItemAsync(defaultContent);
    }

    /// <inheritdoc/>       
    public override async Task DeactivateItemAsync(IScreen item, bool close, CancellationToken cancellationToken = default)
    {
        await base.DeactivateItemAsync(item, close, cancellationToken);
        //we want the default screen to be activated whenever we deactivate any other screen
        if (!close && (item != defaultContent))
        {
            await this.ActivateItemAsync(defaultContent);
        }
    }
}

[ExcludeFromCodeCoverage]
/// <summary>
/// Dummy screen to show in TestDataRepository host when there is no automation project open and active
/// </summary>
public class MockPrefabExplorerViewModel : Screen
{
}