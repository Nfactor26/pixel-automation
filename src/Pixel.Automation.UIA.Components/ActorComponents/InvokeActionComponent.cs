using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components;

/// <summary>
/// Use <see cref="InvokeActorComponent"/> to perform defult stateless action on a control e.g. click on a button.
/// Control must supported <see cref="InvokePattern"/>.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Invoke", "UIA", iconSource: null, description: "Trigger Invoke pattern on AutomationElement", tags: new string[] { "Invoke","UIA" })]
public class InvokeActorComponent : UIAActorComponent
{
    private readonly ILogger logger = Log.ForContext<InvokeActorComponent>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public InvokeActorComponent():base("Invoke", "Invoke")
    {

    }

    /// <summary>
    /// Perform Invoke action on the control.
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        control.Invoke();
        logger.Information("Invoke performed on control : '{0}'", name);
    }

}
