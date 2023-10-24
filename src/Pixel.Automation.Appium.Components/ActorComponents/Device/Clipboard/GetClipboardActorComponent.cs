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
/// Get the content of the system clipboard
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Clipboard", "Appium", "Device", iconSource: null, description: "Get the content of the system clipboard", tags: new string[] { "clipboard", "get" })]
public class GetClipboardActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetClipboardActorComponent>();

    [DataMember]
    [Display(Name = "Content Type", GroupName = "Input", Order = 10, Description = "Clipboard content type")]
    public ClipboardContentType ContentType { get; set; } = ClipboardContentType.PlainText;

    [DataMember]
    [Display(Name = "Content", GroupName = "Output", Order = 10, Description = "Retrieved clipboard content")]
    [AllowedTypes(typeof(string), typeof(Image))]
    public Argument Content { get; set; } = new OutArgument<string>() { CanChangeType = true };

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetClipboardActorComponent() : base("Get Clipboard", "GetClipboard")
    {

    }

    /// <summary>
    /// Get the content of the system clipboard
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
                    string plainText = androidDriver.GetClipboardText();
                    await this.ArgumentProcessor.SetValueAsync<string>(this.Content, plainText);
                    logger.Information("Retrieved text : '{0}' from clipboard", plainText);
                    break;
                case ClipboardContentType.Url:
                    string url = androidDriver.GetClipboardUrl();
                    await this.ArgumentProcessor.SetValueAsync<string>(this.Content, url);
                    logger.Information("Retrieved url : '{0}' from clipboard", url);
                    break;
                case ClipboardContentType.Image:
                    await this.ArgumentProcessor.SetValueAsync<Image>(this.Content, androidDriver.GetClipboardImage());
                    logger.Information("Image was retrieved from clipboard");
                    break;
            }

        }
        else if (driver is IOSDriver iOSDriver)
        {
            switch (this.ContentType)
            {
                case ClipboardContentType.PlainText:
                    string plainText = iOSDriver.GetClipboardText();
                    await this.ArgumentProcessor.SetValueAsync<string>(this.Content, iOSDriver.GetClipboardText());
                    logger.Information("Retrieved text : '{0}' from clipboard", plainText);
                    break;
                case ClipboardContentType.Url:
                    string url = iOSDriver.GetClipboardUrl();
                    await this.ArgumentProcessor.SetValueAsync<string>(this.Content, iOSDriver.GetClipboardUrl());
                    logger.Information("Retrieved url : '{0}' from clipboard", url);
                    break;
                case ClipboardContentType.Image:
                    await this.ArgumentProcessor.SetValueAsync<Image>(this.Content, iOSDriver.GetClipboardImage());
                    logger.Information("Image was retrieved from clipboard");
                    break;
            }
        }       
    }
}