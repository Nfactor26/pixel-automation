using Pixel.Automation.Core.Attributes;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components.Android;

/// <summary>
/// Android application that can be automated using Appium
/// </summary>
[DataContract]
[Serializable]
[DisplayName("Android")]
[Description("Android applications using Appium")]
[ControlLocator(typeof(AppiumNativeControlLocatorComponent))]
[ControlLocator(typeof(AppiumWebControlLocatorComponent))]
[ApplicationEntity(typeof(AppiumApplicationEntity))]
[Builder(typeof(AppiumControlIdentityBuilder))]
[SupportedPlatforms("WINDOWS", "LINUX", "OSX")]
public class AndroidApplication : AppiumApplication
{
    /// <summary>
    /// constructor
    /// </summary>
    public AndroidApplication() : base()
    {
        this.PlatformName = "Android";
        this.AutomationName = "UIAutomator2";
    }
}
