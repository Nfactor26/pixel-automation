using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components;

/// <summary>
/// Use <see cref="GetTextActorComponent"/> to get the text of a control.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Text", "Java", iconSource: null, description: "Get text contents of a control", tags: new string[] { "Get Text", "Java" })]
public class GetTextActorComponent : JABActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetTextActorComponent>();

    /// <summary>
    /// Argument where the value of the retrieve text will be stored
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Argument where the value of the retrieved text will be stored")]
    public Argument Result { get; set; } = new OutArgument<string>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetTextActorComponent() : base("Get Text", "GetText")
    {

    }

    /// <summary>
    /// Retrieve the text of a control
    /// </summary>
    public override async Task ActAsync()
    {          
        var (name, targetControl) = await this.GetTargetControl();
        var info = targetControl.GetInfo();
        if ((info.accessibleInterfaces & AccessibleInterfaces.cAccessibleTextInterface) != 0)
        {               
            AccessibleTextInfo textInfo;
            targetControl.AccessBridge.Functions.GetAccessibleTextInfo(targetControl.JvmId, targetControl.AccessibleContextHandle, out textInfo, 0, 0);
            var reader = new AccessibleTextReader(targetControl, textInfo.charCount);
            var lines = reader
              .ReadFullLines(targetControl.AccessBridge.TextLineLengthLimit)
              .Where(x => !x.IsContinuation)
              .Take(targetControl.AccessBridge.TextLineCountLimit);
            StringBuilder sb = new StringBuilder();
            foreach (var lineData in lines)
            {                   
                sb.Append(lineData.Text);
            }
            string retrievedText = sb.ToString();
            await this.ArgumentProcessor.SetValueAsync<string>(this.Result, retrievedText);
            logger.Information("Retrieved text : '{0}' from control : '{1}'", retrievedText, name);
            return;
        }

        throw new InvalidOperationException($"Control: {this.TargetControl} doesn't support cAccessibleTextInterface.");
    }

    //private static string MakePrintable(string text)
    //{
    //    var sb = new StringBuilder();
    //    foreach (var ch in text)
    //    {
    //        if (ch == '\n') sb.Append("\\n");
    //        else if (ch == '\r') sb.Append("\\r");
    //        else if (ch == '\t') sb.Append("\\t");
    //        else if (char.IsControl(ch)) sb.Append("#");
    //        else sb.Append(ch);
    //    }
    //    return sb.ToString();
    //}
}
