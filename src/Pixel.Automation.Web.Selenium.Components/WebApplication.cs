using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Web.Selenium.Components.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

[DataContract]
[Serializable]  
[DisplayName("Browser Selenium")]
[Description("Browser based web applications using Selenium")]
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
    [Display(Name = "Preferred Browser", GroupName = "Browser", Order = 30, Description = "Browser to use")]
    public Browsers PreferredBrowser { get; set; }
    
    /// <summary>
    /// Web application url
    /// </summary>
    [DataMember(IsRequired = true)]    
    [Display(Name = "Target Url", GroupName = "Browser", Order = 40, Description = "Web application url")]
    public Uri TargetUri { get; set; } = new Uri("https://www.bing.com");

    /// <summary>
    /// Indicates whether the browser should be maximized on launch. Default value is true.
    /// </summary>
    [DataMember]
    [Display(Name = "Maximize browser", GroupName = "Browser",  Order = 50, Description = "Specify whether browser window should be maximized after launch")]    
    public bool MaximizeOnLaunch { get; set; } = true;

    /// <summary>
    /// Set to true if connecting to Selenium Grid
    /// </summary>
    [DataMember]
    [Display(Name = "Use Grid", GroupName = "Grid", Order = 10, Description = "Set to true if connecting to selenium grid")]
    public bool UseGrid { get; set; } = false;

    /// <summary>
    /// Url of the selenium grid server
    /// </summary>
    [DataMember]
    [Display(Name = "Grid Url", GroupName = "Grid", Order = 20, Description = "Url of the selenium grid server")]
    public Uri GridUrl { get; set; } = new Uri("https://localhost:4444");

    /// <summary>
    /// <see cref="IWebDriver"/> used to interact with the browser
    /// </summary>
    [Browsable(false)]
    [IgnoreDataMember]
    public IWebDriver WebDriver { get; set; }     
  
}
