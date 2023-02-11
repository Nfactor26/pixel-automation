namespace Pixel.Automation.Appium.Scrapper;

/// <summary>
/// OptionRow is used to capture appium desired capabilities to start a new session
/// </summary>
public class OptionRow
{
    /// <summary>
    /// Key for the row
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Type of data
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Value for the Key
    /// </summary>
    public string Value { get; set; }
}
