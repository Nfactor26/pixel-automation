namespace Pixel.Persistence.Core.Request;

/// <summary>
/// Request data to add a new control for a specified application screen
/// </summary>
/// <param name="ScreenName"></param>
/// <param name="ControlData"></param>
public record AddControlRequest(string ApplicationId, string ControlId, string ScreenId, object ControlData);

