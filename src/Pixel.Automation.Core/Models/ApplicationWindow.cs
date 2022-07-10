using Pixel.Automation.Core.Controls;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pixel.Automation.Core.Models
{
    public class ApplicationWindow
    {    
        [Display(Name = "Window Title", Order = 10)]
        public string WindowTitle { get; set; }
      
        [Display(Name = "Process Id", Order = 20)]
        public int ProcessId { get; set; }
      
        [Display(Name = "Window Handle", Order = 30)]
        public IntPtr HWnd { get; set; }

     
        [Display(Name = "Window Bounds", Order = 40)]
        public BoundingBox WindowBounds { get; set; }     
       
        [Display(Name = "Is Visible", Order = 50)]
        public bool IsVisible { get; set; }       

        private ApplicationWindow()
        {

        }

        public ApplicationWindow(int processId, IntPtr handle, string windowTitle, BoundingBox windowBounds, bool isVisible)
        {
            this.ProcessId = processId;
            this.HWnd = handle;
            this.WindowTitle = windowTitle;
            this.WindowBounds = windowBounds;
            this.IsVisible = isVisible;
        }
    }
}
