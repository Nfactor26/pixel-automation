using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;

namespace Pixel.Automation.Appium.Scrapper;

/// <summary>
/// ControlNode wraps a <see cref="MobileControl"/> and provides a tree view of control hierarchy
/// </summary>
public class ControlNode : NotifyPropertyChanged
{
    /// <summary>
    /// Display Text for the control node
    /// </summary>
    public string DisplayText
    {
        get => Control.ToString();
    }

    /// <summary>
    /// Indicates if the control node is selected
    /// </summary>
    public bool IsSelected { get; set; }   

    /// <summary>
    /// Children nodes of the control
    /// </summary>
    public BindableCollection<ControlNode> Children { get; private set; } = new();

    /// <summary>
    /// Wrapped mobile control
    /// </summary>
    public MobileControl Control { get; private set; }

    private BoundingBox actualBoundingBox;
    /// <summary>
    /// Bounding box of the control in mobile coordinates
    /// </summary>
    public BoundingBox ActualBoundingBox
    {
        get
        {
            if(this.actualBoundingBox == null)
            {
                this.actualBoundingBox = new BoundingBox(Control.BoundingBox.X, Control.BoundingBox.Y, Control.BoundingBox.Width, Control.BoundingBox.Height);
            }
            return this.actualBoundingBox;
        }
    }
 
    /// <summary>
    /// Bounding box of the control in desktop screen coordinates
    /// </summary>
    public BoundingBox ScaledBoundingBox { get; set; }
   
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="control"></param>
    public ControlNode(MobileControl control)
    {
        this.Control = control;
    }
    
    /// </inheritdoc> 
    public override string ToString()
    {
        return Control.ToString();
    }

}
