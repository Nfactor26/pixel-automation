using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public class ApplicationWindow
    {
        [DataMember]
        [Display(Name = "Window Title", Order = 10)]
        public string WindowTitle { get; set; }

        [DataMember]
        [Display(Name = "Process Id", Order = 20)]
        public int ProcessId { get; set; }

        [DataMember]
        [Display(Name = "Window Handle", Order = 30)]
        public IntPtr HWnd { get; set; }

        [DataMember]
        [Display(Name = "Window Bounds", Order = 40)]
        public Rectangle WindowBounds { get; set; }      

        [DataMember]
        [Display(Name = "Is Visible", Order = 50)]
        public bool IsVisible { get; set; }       

        private ApplicationWindow()
        {

        }

        public ApplicationWindow(int processId, IntPtr handle, string windowTitle, Rectangle windowBounds, bool isVisible)
        {
            this.ProcessId = processId;
            this.HWnd = handle;
            this.WindowTitle = windowTitle;
            this.WindowBounds = windowBounds;
            this.IsVisible = isVisible;
        }
    }
}
