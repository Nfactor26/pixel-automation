using Pixel.Automation.Core.Interfaces;
using System;
using System.Drawing;

namespace Pixel.Automation.Core.Controls
{
    public class ScrapedControl : IDisposable
    {
        /// <summary>
        /// Captured image for the control while scraping
        /// </summary>
        public Bitmap ControlImage { get; set; }

        /// <summary>
        /// Processed control data in the form of <see cref="IControlIdentity"/>
        /// </summary>
        public object? ControlData { get; set; }      

        /// <summary>
        /// Destructor
        /// </summary>
        ~ScrapedControl()
        {
            Dispose(false);
        }

        /// </inheritdoc>        
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
