using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Set the content of the system clipboard
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Set Clipboard", "Appium", "Device", iconSource: null, description: "Set the content of the system clipboard", tags: new string[] { "clipboard", "set" })]
public class SetClipboardActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<SetClipboardActorComponent>();

    [DataMember]
    [Display(Name = "Content Type", GroupName = "Input", Order = 10, Description = "Clipboard content type")]
    public ClipboardContentType ContentType { get; set; } = ClipboardContentType.PlainText;

    [DataMember]
    [Display(Name = "Content", GroupName = "Input", Order = 20, Description = "Content to set")]
    [AllowedTypes(typeof(string), typeof(Image))]
    public Argument Content { get; set; } = new InArgument<string>() { CanChangeType = true, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound  };

    [DataMember]
    [Display(Name = "Label", GroupName = "Input", Order = 30, Description = "User visible label for the clipboard content. Android only.")]
    [AllowedTypes(typeof(string), typeof(Image))]
    public Argument Label { get; set; } = new InArgument<string>() { CanChangeType = true, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Default constructor
    /// </summary>
    public SetClipboardActorComponent() : base("Set Clipboard", "SetClipboard")
    {

    }

    /// <summary>
    /// Set the content of the system clipboard
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        if (driver is AndroidDriver androidDriver)
        {
            switch (this.ContentType)
            {
                case ClipboardContentType.PlainText:
                    string textContent = await this.ArgumentProcessor.GetValueAsync<string>(this.Content);
                    string label = await this.ArgumentProcessor.GetValueAsync<string>(this.Label);
                    androidDriver.SetClipboardText(textContent, label);
                    logger.Information("Clipboard content set to text : '{0}' with label : '{1}'", textContent, label);
                    break;
                case ClipboardContentType.Url:
                    string urlContent = await this.ArgumentProcessor.GetValueAsync<string>(this.Content);
                    androidDriver.SetClipboardUrl(urlContent);
                    logger.Information("Clipboard content set to url : '{0}'", urlContent);
                    break;
                case ClipboardContentType.Image:
                    androidDriver.SetClipboardImage((await this.ArgumentProcessor.GetValueAsync<Image>(this.Content)));
                    logger.Information("Clipboard content set to image");
                    break;
            }
        }
        else if (driver is IOSDriver iOSDriver)
        {
            switch (this.ContentType)
            {
                case ClipboardContentType.PlainText:
                    string content = await this.ArgumentProcessor.GetValueAsync<string>(this.Content);
                    string label = await this.ArgumentProcessor.GetValueAsync<string>(this.Label);
                    iOSDriver.SetClipboardText(content, label);
                    logger.Information("Clipboard content set to text : '{0}' with label : '{1}'", content, label);
                    break;
                case ClipboardContentType.Url:                   
                    string urlContent = await this.ArgumentProcessor.GetValueAsync<string>(this.Content);
                    iOSDriver.SetClipboardUrl(urlContent);
                    logger.Information("Clipboard content set to url : '{0}'", urlContent);
                    break;
                case ClipboardContentType.Image:
                    iOSDriver.SetClipboardImage((await this.ArgumentProcessor.GetValueAsync<Image>(this.Content)));
                    logger.Information("Clipboard content set to image");
                    break;
            }
        }      
    }
}