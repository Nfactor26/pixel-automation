using OpenQA.Selenium.Appium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Get the current geolocation
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get GeoLocation", "Appium", "Session", iconSource: null, description: "Get the current geo location", tags: new string[] { "geolocation", "get location" })]
public class GetGeoLocationActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetOrientationActorComponent>();

    [DataMember]
    [Display(Name = "GeoLocation", GroupName = "Output", Order = 10, Description = "Current geolocation")]
    public Argument GeoLocation { get; set; } = new OutArgument<Location>() { CanChangeType = false };

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetGeoLocationActorComponent() : base("Get GeoLocation", "GetGeoLocation")
    {

    }

    /// <summary>
    /// Get the current geolocation
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var location = driver.Location;
        await this.ArgumentProcessor.SetValueAsync<Location>(this.GeoLocation, location);
        logger.Information("Current device geolocation is Latitude:{0}, Longitude:{1}, Altitude:{2}", location.Latitude, location.Longitude, location.Altitude);
    }
}