using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IControlScrapper
    {     
        /// <summary>
        /// True if monitoring controls otherwise false
        /// </summary>
        bool IsCapturing
        {
            get;
        }

        /// <summary>
        /// Display name of the Scrapper
        /// </summary>
        string DisplayName
        {
            get;
        }

        /// <summary>
        /// Toggle between StartCapture and StopCapture
        /// </summary>
        /// <returns></returns>
        Task ToggleCapture();

        /// <summary>
        /// Start monitoring controls in order to capture them 
        /// </summary>
        Task StartCapture();      

        /// <summary>
        /// Stop monitoring controls 
        /// </summary>
        Task StopCapture();

        /// <summary>
        /// Get all the controls that have been captured 
        /// </summary>
        /// <returns></returns>
        IEnumerable<Object> GetCapturedControls();
    }
}
