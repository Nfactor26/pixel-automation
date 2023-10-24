using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="GotoActorComponent"/> to navigate the browser to a specified url.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Goto", "Appium", "Browser", iconSource: null, description: "Navigate the browser to desired url", tags: new string[] { "goto" })]
public class GotoActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<GotoActorComponent>();

    /// <summary>
    /// Url to which active window/tab should be navigated
    /// </summary>
    [DataMember(IsRequired = true)]   
    [Display(Name = "Url", GroupName = "Configuration", Order = 10, Description = "Url to navigate to")]
    [AllowedTypes(typeof(string), typeof(Uri))]
    public Argument TargetUrl { get; set; } = new InArgument<string>() { DefaultValue = "https://www.bing.com" , CanChangeType = true };

    /// <summary>
    /// Default constructor
    /// </summary>
    public GotoActorComponent():base("Goto", "Goto")
    {

    }

    /// <summary>
    /// Navigate the active window/tab of the brower to a configured url.
    /// </summary>
    public override async Task ActAsync()
    {
        switch(this.TargetUrl)
        {
            case InArgument<string>:
                var targetUrlString = await ArgumentProcessor.GetValueAsync<string>(this.TargetUrl);
                this.ApplicationDetails.Driver.Navigate().GoToUrl(targetUrlString);
                logger.Information("Browser was avigated to url : '{0}'", targetUrlString);
                break;
            case InArgument<Uri>:
                var targetUrlUri = await ArgumentProcessor.GetValueAsync<Uri>(this.TargetUrl);
                this.ApplicationDetails.Driver.Navigate().GoToUrl(targetUrlUri);
                logger.Information("Browser was navigated to url : '{0}'", targetUrlUri);
                break;
        }          
        await Task.CompletedTask;
    }

}
