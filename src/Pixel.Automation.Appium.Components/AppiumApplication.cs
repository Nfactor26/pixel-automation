using OpenQA.Selenium.Appium;
using Pixel.Automation.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Base class for any application based on Appium
/// </summary>
[DataContract]
[Serializable]
public abstract class AppiumApplication : Application
{
    [DataMember(IsRequired = true, Order = 10)]
    [Display(Name = "Platform Name", GroupName = "Application", Order = 10, Description = "platformName capability value")]
    public string PlatformName { get; set; }

    [DataMember(IsRequired = true, Order = 20)]
    [Display(Name = "Platform Version", GroupName = "Application", Order = 20, Description = "appium:plataformVersion capability value")]
    public string PlatformVersion { get; set; }

    [DataMember(IsRequired = true, Order = 30)]
    [Display(Name = "Automation Name", GroupName = "Application", Order = 30, Description = "appium:AutomationNaame capability value")]
    public string AutomationName { get; set; }

    [DataMember(IsRequired = true, Order = 40)]
    [Display(Name = "Device Name", GroupName = "Application", Order = 40, Description = "appium:deviceName capability value")]
    public string DeviceName { get; set; }
  
    [DataMember(IsRequired = true, Order = 50)]
    [Display(Name = "App", GroupName = "Application", Order = 50, Description = "appium:app capability value")]
    public string App { get; set; }      

    [DataMember(IsRequired = true, Order = 1000)]
    [Display(Name = "Additional Capabilites", GroupName = "Application", Order = 1000, Description = "Any additional appium capabilities")]
    public Dictionary<string, string> Capabilities { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// <see cref="AppiumDriver<>"/>
    /// </summary>
    [Browsable(false)]
    [IgnoreDataMember]
    public virtual AppiumDriver Driver { get; set; }
}
