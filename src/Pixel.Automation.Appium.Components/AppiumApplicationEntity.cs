using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Support.Extensions;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// ApplicationEntity for Appium based applications
/// </summary>
public class AppiumApplicationEntity : ApplicationEntity
{
    protected bool useRemoteServer = true;
    [DataMember(IsRequired = true, Order = 1000)]
    [Display(Name = "Use Remote Addressss", GroupName = "Server", Order = 10, Description = "Indicates whether to use a remote server or start a server locally")]
    [RefreshProperties(RefreshProperties.Repaint)]
    public bool UseRemoteServer
    {
        get => this.useRemoteServer;
        set
        {
            this.useRemoteServer = value;
            this.SetDispalyAttribute(nameof(RemoteAddress), value);
            this.SetDispalyAttribute(nameof(ServiceBuilderOverride), !value);
            OnPropertyChanged();
        }
    }

    [DataMember(IsRequired = true)]
    [Display(Name = "Remote Server Address", GroupName = "Server", Order = 20, Description = "Address of the appium remote server")] 
    public Argument RemoteAddress { get; set; } = new InArgument<Uri> { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = new Uri("http://127.0.0.1:4723") };

    [DataMember(IsRequired = true)]
    [Display(Name = "Service Builder", GroupName = "Server", Order = 30, Description = "[Optional] Specify a custom AppiumServiceBuilder")]   
    public Argument ServiceBuilderOverride { get; set; } = new InArgument<AppiumServiceBuilder> { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound, CanChangeType = false };

    [DataMember(IsRequired = true)]
    [Display(Name = "Command Timeout", GroupName = "Server", Order = 40, Description = "Command timeout value")]
    public Argument CommandTimeOut { get; set; } = new InArgument<TimeSpan> { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = TimeSpan.FromSeconds(30) };

    [DataMember(IsRequired = true)]
    [Display(Name = "Appium Options", GroupName = "Options", Order = 20, Description = "[Optional] Specify custom Appium DriverOptions. This will discard any application level settings and defined Capabilities.")]
    public Argument AppiumOptionsOverride { get; set; } = new InArgument<AppiumOptions> { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound, CanChangeType = false };

    [DataMember(IsRequired = true)]
    [Display(Name = "Capabilites", GroupName = "Options", Order = 10, Description = "[Optional] Appium desired capabilities. This is combined with capabilities defined on application and will override any existsing values.")]
    public Argument CapabilitiesOverride { get; set; } = new InArgument<Dictionary<string, string>> { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound, CanChangeType = false };
       
    /// </inheritdoc>
    public override async Task LaunchAsync()
    {
        var applicationDetails = this.GetTargetApplicationDetails<AppiumApplication>();
        var driverOptions = await GetDriverOptions(applicationDetails);
        var commandTimeout = await this.ArgumentProcessor.GetValueAsync<TimeSpan>(this.CommandTimeOut);
        if(this.useRemoteServer)
        {
            var remoteAddress = await this.ArgumentProcessor.GetValueAsync<Uri>(this.RemoteAddress);
            applicationDetails.Driver = new OpenQA.Selenium.Appium.Android.AndroidDriver(remoteAddress, driverOptions, commandTimeout);
        }
        else
        {
            var serviceBuilder = await GetServiceBuilder();
            applicationDetails.Driver = new OpenQA.Selenium.Appium.Android.AndroidDriver(serviceBuilder, driverOptions, commandTimeout);
        }
    
    }

    /// </inheritdoc>
    public override async Task CloseAsync()
    {
        var webApplicationDetails = this.GetTargetApplicationDetails<AppiumApplication>();
        if (webApplicationDetails.Driver != null)
        {
            webApplicationDetails.Driver.Quit();
            webApplicationDetails.Driver = null;
            await Task.CompletedTask;
        }
    }

    /// </inheritdoc>
    public override async Task CaptureScreenShotAsync(string filePath)
    {       
        if (this.AllowCaptureScreenshot)
        {          
            var imageManager = this.EntityManager.GetServiceOfType<IImageManager>();
            var appiumDriver = this.GetTargetApplicationDetails<AppiumApplication>().Driver;
            var screenShot = appiumDriver.TakeScreenshot();
            await imageManager.SaveAsAsync(screenShot.AsByteArray, filePath, Core.Enums.ImageFormat.Jpeg);
        }    
    }

    async Task<AppiumServiceBuilder> GetServiceBuilder()
    {
        AppiumServiceBuilder builder;
        if (this.ServiceBuilderOverride.IsConfigured())
        {
            logger.Information($"AppiumServiceBuilder was over-ridden for application : {applicationDetails.ApplicationName}");
            builder = await this.ArgumentProcessor.GetValueAsync<AppiumServiceBuilder>(this.ServiceBuilderOverride);
            return builder;
        }
       
        builder = new AppiumServiceBuilder();     
        builder = builder.UsingAnyFreePort();      
        return builder;
    }

    async Task<AppiumOptions> GetDriverOptions(AppiumApplication application)
    {
        AppiumOptions driverOptions;
        if (this.AppiumOptionsOverride.IsConfigured())
        {
            logger.Information($"AppiumOptions was over-ridden for application : {applicationDetails.ApplicationName}");
            driverOptions = await this.ArgumentProcessor.GetValueAsync<AppiumOptions>(this.AppiumOptionsOverride);
            return driverOptions;
        }

        var capabilities = new Dictionary<string, string>(application.Capabilities);
        if (this.CapabilitiesOverride.IsConfigured())
        {
            var capabilitiesOverride = await this.ArgumentProcessor.GetValueAsync<Dictionary<string, string>>(this.CapabilitiesOverride);
            foreach(var capability in capabilitiesOverride)
            {
                if(capabilities.ContainsKey(capability.Key))
                {
                    capabilities[capability.Key] = capability.Value;
                }
                else
                {
                    capabilities.Add(capability.Key, capability.Value);
                }
            }
        }

        driverOptions = new AppiumOptions()
        {
            AutomationName = application.AutomationName,
            DeviceName = application.DeviceName,           
            PlatformVersion = application.PlatformVersion,
            App = application.App
        };
        driverOptions.AddAdditionalAppiumOption(nameof(AppiumOptions.PlatformName), application.PlatformName);
        foreach(var capability in capabilities)
        {
            driverOptions.AddAdditionalAppiumOption(capability.Key, capability.Value);
        }
        return driverOptions;
    }

}
