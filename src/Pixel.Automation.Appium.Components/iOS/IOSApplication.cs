//using Pixel.Automation.Appium.Components.Controls;
//using Pixel.Automation.Core.Attributes;
//using System.ComponentModel;
//using System.Runtime.Serialization;

//namespace Pixel.Automation.Appium.Components.iOS;

///// <summary>
///// iOS application that can be automated using Appium
///// </summary>
//[DataContract]
//[Serializable]
//[DisplayName("iOS")]
//[Description("iOS applications using Appium")]
//[ControlLocator(typeof(AppiumNativeControlLocatorComponent))]
//[ApplicationEntity(typeof(AppiumApplicationEntity))]
//[Builder(typeof(AppiumControlIdentityBuilder))]
//public class IOSApplication : AppiumApplication
//{
//    /// <summary>
//    /// constructor
//    /// </summary>
//    public IOSApplication() : base()
//    {
//        this.PlatformName = "iOS";
//        this.AutomationName = "XCUITest";
//    }
//}
