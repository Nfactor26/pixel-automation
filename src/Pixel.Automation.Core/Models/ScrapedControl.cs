using Pixel.Automation.Core.Interfaces;
using System;
using System.Drawing;

namespace Pixel.Automation.Core.Models
{
    public class ScrapedControl : IDisposable
    {
        public Bitmap ControlImage { get; set; }

        public IControlIdentity ControlData { get; set; }

        ~ScrapedControl()
        {
            Dispose(false);
        }

        public void Dispose()
        {         
            Dispose(true);          
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ControlImage?.Dispose();
            }           
        }

    }  
}
