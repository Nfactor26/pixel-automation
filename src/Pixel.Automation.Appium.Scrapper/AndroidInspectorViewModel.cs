using Microsoft.Win32;
using Notifications.Wpf.Core;
using OpenQA.Selenium.Appium;
using Pixel.Automation.Core.Interfaces;
using System;
using System.IO;
using System.Xml;

namespace Pixel.Automation.Appium.Scrapper;

/// <summary>
/// InspectorViewModel for the Android device
/// </summary>
public class AndroidInspectorViewModel : InspectorViewModel
{
    protected string application;
    /// <summary>
    /// Path of the apk file 
    /// </summary>
    public string Application
    {
        get => this.application;
        set
        {
            this.application = value;
            NotifyOfPropertyChange();
        }
    }


    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="highlightRectangle"></param>
    /// <param name="notifcationManager"></param>
    public AndroidInspectorViewModel(IHighlightRectangle highlightRectangle, INotificationManager notifcationManager) : base(highlightRectangle, notifcationManager)
    {
        this.appiumOptions = new AppiumOptions()
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2",
            DeviceName = "emulator-5554"
        };
        this.DesiredCapabilities.Add(new OptionRow() { Key = "appium:platformName", Type = "text", Value = this.appiumOptions.PlatformName });
        this.DesiredCapabilities.Add(new OptionRow() { Key = "appium:automationName", Type = "text", Value = this.appiumOptions.AutomationName });
        this.DesiredCapabilities.Add(new OptionRow() { Key = "appium:deviceName", Type = "text", Value = this.appiumOptions.DeviceName });       
        this.DesiredCapabilities.Add(new OptionRow() { Key = "appium:newCommandTimeout", Type = "number", Value = "7200" });
    }

    /// </inheritdoc>
    protected override AppiumDriver CreateDriver()
    {
        if(!string.IsNullOrEmpty(this.Application))
        {
            if(!File.Exists(this.Application))
            {

            }
            this.appiumOptions.App = this.Application;
        }
        foreach(var capability in this.DesiredCapabilities)
        {
            if(string.IsNullOrEmpty(capability.Key))
            {
                continue;
            }
            switch(capability.Key)
            {
                case "appium:automationName":
                    this.appiumOptions.AutomationName = capability.Value;
                    break;
                case "appium:deviceName":
                    this.appiumOptions.DeviceName = capability.Value;
                    break;
                case "appium:platformName":
                    break;
                case "appium:app":
                    this.appiumOptions.App = capability.Value;
                    break;
                case "appium:platformVersion":
                    this.appiumOptions.PlatformVersion = capability.Value;
                    break;
                case "appium:browserName":
                    this.appiumOptions.BrowserName = capability.Value;
                    break;
                default:
                    switch(capability.Type)
                    {
                        case "text":
                            this.appiumOptions.AddAdditionalAppiumOption(capability.Key, capability.Value);
                            break;
                        case "boolean":
                            this.appiumOptions.AddAdditionalAppiumOption(capability.Key, bool.Parse(capability.Value));
                            break;
                        case "number":
                            this.appiumOptions.AddAdditionalAppiumOption(capability.Key, int.Parse(capability.Value));
                            break;
                        case "JSON Object":
                            this.appiumOptions.AddAdditionalAppiumOption(capability.Key, capability.Value);
                            break;
                    }
                    break;
            }
        }  
        return new OpenQA.Selenium.Appium.Android.AndroidDriver(new Uri(this.remoteUrl), this.appiumOptions, TimeSpan.FromMinutes(30));
    }

    /// </inheritdoc>
    protected override MobileControl CreateMobileControl(XmlReader reader)
    {
        return new AndroidMobileControl(reader);
    }

    /// <summary>
    /// Show a open file dialog to pick Android package files (*.apk)
    /// </summary>
    public void PickApplication()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Android Package File (*.apk)|*.apk";
        openFileDialog.InitialDirectory = Environment.CurrentDirectory;
        if (openFileDialog.ShowDialog() == true)
        {
            this.Application = openFileDialog.FileName;
            logger.Information("File {0} was selected", this.Application);
        }
    }

}
