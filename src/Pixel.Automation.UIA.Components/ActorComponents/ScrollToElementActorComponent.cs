using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components;

/// <summary>
/// Use <see cref="ScrollToActorComponent"/> to scroll to a target control
/// Control must support <see cref="ScrollItemPattern"/>
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Scroll To Control", "UIA", iconSource: null, description: "Scroll control in to view", tags: new string[] { "Scroll", "UIA" })]
public class ScrollToActorComponent : UIAActorComponent
{
    private readonly ILogger logger = Log.ForContext<ScrollToActorComponent>();
 
    /// <summary>
    /// Default constructor
    /// </summary>
    public ScrollToActorComponent() : base("Scroll To Element", "ScrollToElement")
    {

    }

    /// <summary>
    /// Scroll control in to view.       
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws InvalidOperationException if ScrollItemPattern is not supported</exception>   
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        control.ScrollIntoView();
        logger.Information("Control : '{0}' was scrolled in to view", name);
    }

}
