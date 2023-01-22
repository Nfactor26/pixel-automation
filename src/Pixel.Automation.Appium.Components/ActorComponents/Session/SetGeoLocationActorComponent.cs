using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Set the current geo location
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Set GeoLocation", "Appium", "Session", iconSource: null, description: "Set the current geo location", tags: new string[] { "geolocation", "set geolocation" })]
public class SetGeoLocationActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<SetGeoLocationActorComponent>();

    [DataMember]
    [Display(Name = "Latitude", GroupName = "Input", Order = 10, Description = "Latitude component of geo location")]
    public Argument Latitude { get; set; } = new InArgument<double>() { Mode = ArgumentMode.Default, CanChangeType = false , DefaultValue = 0 };

    [DataMember]
    [Display(Name = "Longitude", GroupName = "Input", Order = 20, Description = "Longitude component of geo location")]
    public Argument Longitude { get; set; } = new InArgument<double>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = 0 };

    [DataMember]
    [Display(Name = "Altitude", GroupName = "Input", Order = 30, Description = "Altitude component of geo location")]
    public Argument Altitude { get; set; } = new InArgument<double>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = 0 };

    /// <summary>
    /// Default constructor
    /// </summary>
    public SetGeoLocationActorComponent() : base("Set GeoLocation", "SetGeoLocation")
    {

    }

    /// <summary>
    /// Set the current geo location
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var latitude = await this.ArgumentProcessor.GetValueAsync<double>(this.Latitude);
        var longitude = await this.ArgumentProcessor.GetValueAsync<double>(this.Longitude);
        var altitude = await this.ArgumentProcessor.GetValueAsync<double>(this.Altitude);
        driver.Location = new OpenQA.Selenium.Appium.Location()
        {
            Latitude = latitude,
            Longitude = longitude,
            Altitude = altitude
        };
        logger.Information("Device location was changed to Latitude:{0}, Longitude:{1}, Altitude:{2}", latitude, longitude, altitude);        
    }
}
