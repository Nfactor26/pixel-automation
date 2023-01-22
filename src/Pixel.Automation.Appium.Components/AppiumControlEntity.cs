using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Serilog;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// ControlEntity for any control based on Appium 
/// </summary>
public class AppiumControlEntity : ControlEntity
{
    private readonly ILogger logger = Log.ForContext<AppiumControlEntity>();

    private UIControl control;

    /// <summary>
    /// Clear the located control once entity is processed
    /// </summary>
    public override async Task OnCompletionAsync()
    {
        if (CacheControl)
        {
            control = null;
            logger.Debug($"Cleared cached AndroidElement for {this.Name}");
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get first control identified using wrapped <see cref="IControlIdentity"/>
    /// </summary>
    /// <returns></returns>
    public override async Task<UIControl> GetControl()
    {
        if (CacheControl && control != null)
        {
            logger.Debug($"Return cached element for {this.Name}");
            return control;
        }


        UIControl searchRoot = default;
        if (this.SearchRoot.IsConfigured())
        {
            searchRoot = await this.ArgumentProcessor.GetValueAsync<UIControl>(this.SearchRoot);
        }
        else if (this.Parent is AppiumControlEntity controlEntity)
        {
            searchRoot = await controlEntity.GetControl();
        }

        var androidControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails);
        switch (LookupMode)
        {
            case LookupMode.FindSingle:
                control = await androidControlLocator.FindControlAsync(this.ControlDetails, searchRoot);
                break;
            case LookupMode.FindAll:
                var descendantControls = await androidControlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);
                switch (FilterMode)
                {
                    case FilterMode.Index:
                        control = await GetElementAtIndex(descendantControls);
                        break;
                    case FilterMode.Custom:
                        control = GetElementMatchingCriteria(descendantControls);
                        break;
                }
                break;
            default:
                throw new NotSupportedException();
        }
        return control;
    }

    /// <summary>
    /// Get all the controls identifed using wrapped <see cref="IControlIdentity"/>
    /// </summary>
    /// <returns></returns>
    public override async Task<IEnumerable<UIControl>> GetAllControls()
    {
        UIControl searchRoot = default;
        if (this.SearchRoot.IsConfigured())
        {
            searchRoot = await this.ArgumentProcessor.GetValueAsync<UIControl>(this.SearchRoot);
        }
        else if (this.Parent is AppiumControlEntity controlEntity)
        {
            searchRoot = await controlEntity.GetControl();
        }
        var controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails);
        var foundControls = await controlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);
        return foundControls;
    }

}
