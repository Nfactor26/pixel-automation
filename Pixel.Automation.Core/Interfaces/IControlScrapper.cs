using System;
using System.Collections.Generic;

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
        /// Start monitoring controls in order to capture them 
        /// </summary>
        void StartCapture();      

        /// <summary>
        /// Stop monitoring controls 
        /// </summary>
        void StopCapture();

        /// <summary>
        /// Get all the controls that have been captured 
        /// </summary>
        /// <returns></returns>
        IEnumerable<Object> GetCapturedControls();
    }
}
