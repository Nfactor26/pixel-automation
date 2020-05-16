using System;
using System.Drawing;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IHighlightRectangle : IDisposable
    {
        /// <summary>
        /// Set the border color of the HighlightRectangle
        /// </summary>
        Color BorderColor { set; }

        /// <summary>
        /// Set the location of the HighlightRectangle relative to top-left of the screen
        /// </summary>
        Rectangle Location { get;  set; }

        /// <summary>
        /// Toggle the visibility of the HighlightRectangle
        /// </summary>
        bool Visible { get; set; }
    }

    public interface IHighlightRectangleFactory
    {
        IHighlightRectangle CreateHighlightRectangle();
    }
}
