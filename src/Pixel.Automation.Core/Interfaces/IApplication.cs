using System;

namespace Pixel.Automation.Core.Interfaces
{   
    public interface IApplication
    {
        /// <summary>
        /// Name of the application
        /// </summary>
        string ApplicationName { get; set; }

        /// <summary>
        /// Unique Id of the application
        /// </summary>
        string ApplicationId { get; set; }

        /// <summary>
        /// ProcessId of the application
        /// </summary>
        int ProcessId { get;}

        /// <summary>
        /// Handle of the application Main Window
        /// </summary>
        IntPtr Hwnd { get; }

        
    }
}
