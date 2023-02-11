using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Xml;

namespace Pixel.Automation.Appium.Scrapper;

/// <summary>
/// Details of android mobile control
/// </summary>
public class AndroidMobileControl : MobileControl
{
    private readonly static Regex boundsMatcher = new Regex("\\d+", RegexOptions.Compiled);

    /// <summary>
    /// Accessibility Id of the control
    /// </summary>
    [Display(Name = "Accessibility Id", GroupName = "Selector", Order = 10, Description = "Accessibility Id")] 
    public string AccessibilityId { get; private set; }

    /// <summary>
    /// XPath of the control
    /// </summary>
    [Display(Name = "XPath", GroupName= "Selector", Order = 20, Description = "Xpath")] 
    public string XPath { get; private set; }

    /// <summary>
    /// Index of the control
    /// </summary>
    [Display(Name = "Index", GroupName = "Attribute", Order = 100, Description = "index")]
    public int Index { get; private set; }

    /// <summary>
    /// Package of the control
    /// </summary>
    [Display(Name = "Package", GroupName = "Attribute", Order = 110, Description = "package")]
    public string Package { get; private set; }

    /// <summary>
    /// Class of the control
    /// </summary>
    [Display(Name = "Class", GroupName = "Attribute", Order = 120, Description = "class")]
    public string Class { get; private set; }

    /// <summary>
    /// Text of the control
    /// </summary>
    [Display(Name = "Text", GroupName = "Attribute", Order = 130, Description = "text")]
    public string Text { get; private set; }

    /// <summary>
    /// Content description of the control
    /// </summary>
    [Display(Name = "Content Description", GroupName = "Attribute", Order = 140, Description = "content-description")]
    public string ContentDescription { get; private set; }

    /// <summary>
    /// Resource Id of the control
    /// </summary>
    [Display(Name = "Resource Id", GroupName = "Attribute", Order = 150, Description = "resource-id")]
    public string ResourceId { get; private set; }

    /// <summary>
    /// Indicates if control can be checked
    /// </summary>
    [Display(Name = "Checkable", GroupName = "Attribute", Order = 200, Description = "checkable")]
    public bool Checkable { get; private set; }

    /// <summary>
    /// Indicates if control is checked
    /// </summary>
    [Display(Name = "Checked", GroupName = "Attribute", Order = 210, Description = "checked")]
    public bool Checked { get; private set; }

    /// <summary>
    /// Indicates if control can be clicked
    /// </summary>
    [Display(Name = "Clickable", GroupName = "Attribute", Order = 220, Description = "cliackable")]
    public bool Clickable { get; private set;  }

    /// <summary>
    /// Indicates if control is enabled
    /// </summary>
    [Display(Name = "Enabled", GroupName = "Attribute", Order = 230, Description = "enabled")]
    public bool Enabled { get; private set; }

    /// <summary>
    /// Indicates if control can be focused
    /// </summary>
    [Display(Name = "Focusable", GroupName = "Attribute", Order = 240, Description = "focusable")]
    public bool Focusable { get; private set; }

    /// <summary>
    /// Indicates if control has focus
    /// </summary>
    [Display(Name = "Focused", GroupName = "Attribute", Order = 250, Description = "focused")]
    public bool Focused { get; private set; }

    /// <summary>
    /// Indicates if control can be long clicked
    /// </summary>
    [Display(Name = "Long Clickable", GroupName = "Attribute", Order = 260, Description = "long-clickable")]
    public bool LongClickable { get; private set; }

    /// <summary>
    /// Indicates if the control is a password control
    /// </summary>
    [Display(Name = "Password", GroupName = "Attribute", Order = 270, Description = "password")]
    public bool Password { get; private set; }

    /// <summary>
    /// Indicates if control can be scrolled
    /// </summary>
    [Display(Name = "Scrollable", GroupName = "Attribute", Order = 280, Description = "scrollable")]
    public bool Scrollable { get; private set; }

    /// <summary>
    /// Indicates if control is selected
    /// </summary>
    [Display(Name = "Selected", GroupName = "Attribute", Order = 290, Description = "selected")]
    public bool Selected { get; private set; }

    /// <summary>
    /// Indicates if control is displayed
    /// </summary>
    [Display(Name = "Displayed", GroupName = "Attribute", Order = 300, Description = "displayed")]
    public bool Displayed { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="controlNode"></param>
    public AndroidMobileControl(XmlReader controlNode) : base(controlNode)
    {       
        if(int.TryParse(controlNode.GetAttribute("index"), out int index))
        {
            this.Index = index;
        }
        this.Package = controlNode.GetAttribute("package") ?? string.Empty;
        this.Class = controlNode.GetAttribute("class") ?? string.Empty;
        this.Text = controlNode.GetAttribute("text") ?? string.Empty;
        this.ContentDescription = controlNode.GetAttribute("content-desc") ?? string.Empty;
        this.ResourceId = controlNode.GetAttribute("resource-id") ?? string.Empty;
        this.Checkable = bool.Parse(controlNode.GetAttribute("checkable") ?? "false");
        this.Checked = bool.Parse(controlNode.GetAttribute("checkable") ?? "false");
        this.Clickable = bool.Parse(controlNode.GetAttribute("clickable") ?? "false");
        this.Enabled = bool.Parse(controlNode.GetAttribute("enabled") ?? "false");
        this.Focusable = bool.Parse(controlNode.GetAttribute("focusable") ?? "false");
        this.Focused = bool.Parse(controlNode.GetAttribute("focused") ?? "false");
        this.LongClickable = bool.Parse(controlNode.GetAttribute("long-clickable") ?? "false");
        this.Password = bool.Parse(controlNode.GetAttribute("password") ?? "false");
        this.Scrollable = bool.Parse(controlNode.GetAttribute("scrollable") ?? "false");
        this.Selected = bool.Parse(controlNode.GetAttribute("selected") ?? "false");
        this.Displayed = bool.Parse(controlNode.GetAttribute("displayed") ?? "false");
        string bounds = controlNode.GetAttribute("bounds");
        if(!string.IsNullOrEmpty(bounds))
        {
            var matches = boundsMatcher.Matches(bounds);
            Debug.Assert(matches.Count.Equals(4));
            this.BoundingBox = new Rectangle(int.Parse(matches[0].Value), int.Parse(matches[1].Value), int.Parse(matches[2].Value) - int.Parse(matches[0].Value), int.Parse(matches[3].Value) - int.Parse(matches[1].Value));        

        }

        if (!string.IsNullOrEmpty(this.Text))
        {
            this.AccessibilityId = this.Text;
        }
        else if (!string.IsNullOrEmpty(this.ResourceId))
        {
            this.AccessibilityId = this.ResourceId;
        }
        else
        {
            this.AccessibilityId = this.ContentDescription;
        }
    }
}
