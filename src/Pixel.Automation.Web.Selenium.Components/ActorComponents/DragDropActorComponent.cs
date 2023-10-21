using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Serilog;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="DragDropActorComponent"/> to drag drop a source web control to target web control.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Drag Drop", "Selenium", iconSource: null, description: "Drag drop a web control to another control", tags: new string[] { "Drag", "Drop", "Web" })]

public class DragDropActorComponent : SeleniumActorComponent
{
    private readonly ILogger logger = Log.ForContext<DragDropActorComponent>();

    /// <summary>
    /// Drag source control
    /// </summary>
    [DataMember]
    [Display(Name = "Drag Source", GroupName = "Control Details", Order = 10)]
    [Description("Drag source control")]
    public Argument SourceControl { get; set; } = new InArgument<UIControl>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Drop target control
    /// </summary>
    [DataMember]
    [Display(Name = "Drop Target", GroupName = "Control Details", Order = 10)]
    [Description("Drop target control")]
    public Argument TargetControl { get; set; } = new InArgument<UIControl>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Default constructor
    /// </summary>
    public DragDropActorComponent() : base("Drag Drop", "DragDrop")
    {

    }

    /// <summary>
    /// Drag the source control and drop over target control
    /// </summary>
    public override async Task ActAsync()
    {            
        var sourceControl = await this.ArgumentProcessor.GetValueAsync<UIControl>(SourceControl);
        var targetControl = await this.ArgumentProcessor.GetValueAsync<UIControl>(TargetControl);

        var sourceWebElement = sourceControl.GetApiControl<IWebElement>();
        var targetWebElement = targetControl.GetApiControl<IWebElement>();
        (new Actions(ApplicationDetails.WebDriver)).DragAndDrop(sourceWebElement, targetWebElement).Perform();
       
        await Task.CompletedTask;

        logger.Information("Source control : '{0}' was drag-dropped to target control : '{1}'", sourceControl.ControlName, targetControl.ControlName);
    }
}
