using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Core.Controls
{
    public class ScrapedControl
    {
        /// <summary>
        /// Captured image for the control while scraping
        /// </summary>
        public byte[] ControlImage { get; set; }

        /// <summary>
        /// Processed control data in the form of <see cref="IControlIdentity"/>
        /// </summary>
        public object? ControlData { get; set; }      

    }  
}
