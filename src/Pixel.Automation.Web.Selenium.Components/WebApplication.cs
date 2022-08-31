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
    /// Indicates whether the browser should be maximized on launch. Default value is true.
    /// </summary>
    [DataMember]
    [Display(Name = "Maximize browser",  Order = 50, Description = "Specify whether browser window should be maximized after launch")]    
    public bool MaximizeOnLaunch { get; set; } = true;
    
    /// <summary>
    /// <see cref="IWebDriver"/> used to interact with the browser
    /// </summary>
    [Browsable(false)]
    [IgnoreDataMember]
    public IWebDriver WebDriver { get; set; }     
  
}
