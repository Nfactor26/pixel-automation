using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Java.Access.Bridge.Components;

public class JavaControlEntity : ControlEntity
{
    private readonly ILogger logger = Log.ForContext<JavaControlEntity>();

    private UIControl control;
  
    /// <summary>
    /// Clear the located control once entity is processed
    /// </summary>
    public override async Task OnCompletionAsync()
    {
        if (!CacheControl)
        {
            control = null;
            logger.Debug($"Cleared cached AccessibleContextNode for {this.Name}");
        }
        await Task.CompletedTask;
    }


    public override async Task<UIControl> GetControl()
    {
        if (CacheControl && control != null)
        {
            logger.Debug($"Return cached AccessibleContextNode for {this.Name}");
            return control;
        }

        UIControl searchRoot = default;
        if (this.SearchRoot.IsConfigured())
        {
            searchRoot = await this.ArgumentProcessor.GetValueAsync<UIControl>(this.SearchRoot);
        }
        else if (this.Parent is JavaControlEntity controlEntity)
        {
            searchRoot = await controlEntity.GetControl();
        }

        var controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails);
        switch (LookupMode)
        {
            case LookupMode.FindSingle:
                control = await controlLocator.FindControlAsync(this.ControlDetails, searchRoot);
                break;
            case LookupMode.FindAll:
                var descendantControls = await controlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);
                switch (FilterMode)
                {
                    case FilterMode.Index:
                        control = await  GetElementAtIndex(descendantControls);
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


    public override async Task<IEnumerable<UIControl>> GetAllControls()
    {
        UIControl searchRoot = default;
        if (this.SearchRoot.IsConfigured())
        {
            searchRoot = await this.ArgumentProcessor.GetValueAsync<UIControl>(this.SearchRoot);
        }
        else if (this.Parent is JavaControlEntity controlEntity)
        {
            searchRoot = await controlEntity.GetControl();
        }
        var controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails);
        var foundControls = await controlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);
        return foundControls;
    }
}
