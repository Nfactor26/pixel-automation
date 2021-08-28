namespace Pixel.Automation.Editor.Core.Interfaces
{
    /// <summary>
    /// Pull screens such as setting window should implement IFlyOut interface
    /// </summary>
    public interface IFlyOut
    {
        /// <summary>
        /// Header Display Text for screen
        /// </summary>
        string Header { get; set; }

        /// <summary>
        /// Indicates whether the screen is currently open
        /// </summary>
        bool IsOpen { get; set; }
    }
}
