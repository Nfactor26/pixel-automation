using Microsoft.Playwright;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

[DataContract]
[Serializable]
[DisplayName("Browser Playwright")]
[Description("Browser based web applications using Playwright")]
[ControlLocator(typeof(WebControlLocatorComponent))]
[ApplicationEntity(typeof(WebApplicationEntity))]
[Builder(typeof(WebControlIdentityBuilder))]
[SupportedPlatforms("WINDOWS", "LINUX", "OSX")]
public class WebApplication : Application
{
    /// <summary>
    /// Browser preference while launching the application
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Preferred Browser", Order = 30, Description = "Browser to use")]
    public Browsers PreferredBrowser { get; set; }

    /// <summary>
    /// Web application url
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Target Url", Order = 40, Description = "Web application url")]
    public Uri TargetUri { get; set; } = new Uri("https://www.bing.com");

    /// <summary>
    /// <see cref="IPlaywright"/> allows launching different types of browsers
    /// </summary>
    [Browsable(false)]
    [IgnoreDataMember]
    public IPlaywright Playwright { get; set; }

    /// <summary>
    /// <see cref="IBrowser"/> used to interact with the browser
    /// </summary>
    [Browsable(false)]
    [IgnoreDataMember]
    public IBrowser Browser { get; set; }

    /// <summary>
    /// If there are multiple Browser contexts created, one of them needs to be set as the Active context.
    /// All interactions happen on the active browser context.
    /// </summary>
    [Browsable(false)]
    [IgnoreDataMember]
    public IBrowserContext ActiveContext { get; set; }

    /// <summary>
    /// Active browser context can have multiple page. One page needs to be set as the active page.
    /// All interactoins happen on the active page of the active browser context.
    /// </summary>
    [Browsable(false)]
    [IgnoreDataMember]
    public IPage ActivePage { get; set; }

}
