using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Xml;

namespace Pixel.Automation.Appium.Scrapper;

/// <summary>
/// Details of a MobileControl
/// </summary>
public abstract class MobileControl
{
    /// <summary>
    /// Indicates if the control is enabled
    /// </summary>
    [Display(Name = "Is Enabled", GroupName = "Attribute", Order = 300, Description = "Indicates if control is enabled")]
    public bool IsEnabled { get; protected set; }

    /// <summary>
    /// Bounding box of the control in mobile screen coordinates
    /// </summary>
    [Display(Name = "Bounding Box", GroupName = "Attribute", Order = 310, Description = "Bounding box of the control")]
    public Rectangle BoundingBox { get; protected set; } = Rectangle.Empty;   

    protected string rawXml;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="controlNode"></param>
    public MobileControl(XmlReader controlNode)
    {       
        this.IsEnabled = bool.Parse(controlNode.GetAttribute("enabled") ?? "false");
        this.rawXml = controlNode.LocalName;
    }

    /// </inheritdoc> 
    public override string ToString()
    {
        return this.rawXml;
    }
}