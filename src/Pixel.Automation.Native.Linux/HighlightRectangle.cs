using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Native.Linux;

public class HighlightRectangle : IHighlightRectangle
{
    public string BorderColor
    {
        set { }
    }
    
    public BoundingBox Location { get; set; } = BoundingBox.Empty;
  
    public bool Visible { get; set; }

    public void Dispose()
    {
        
    }
}