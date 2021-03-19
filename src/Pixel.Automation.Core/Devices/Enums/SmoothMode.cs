namespace Pixel.Automation.Core.Devices
{
    /// <summary>
    /// Control how the mouse movement is done.
    /// </summary>
    public enum SmoothMode
    {
        None,            //Move mouse from point A to B directly without any intermediate stops
        Interpolated     //Divide linear distance between point A and B in to equal intervals and move in multiple steps with  a small delay at each interval
    }
}
