using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// ControlIdentity capturing details of native control belonging to an application on a given platform
/// </summary>
public abstract class AppiumNativeControlIdentity : ControlIdentity
{
    #region Search Strategy

    protected string findByStrategy;
    /// <summary>
    ///  FindBy strategy used to search for a control e.g. id, name, css3selector, etc.     
    /// </summary>   
    [Display(Name = "(Android) Find By", Order = 10, GroupName = "Search Strategy")]
    [Description("FindBy strategy used to search for a control")]
    [DataMember(IsRequired = true, Order = 210)]  
    public virtual string FindByStrategy
    {
        get
        {
            return findByStrategy;
        }
        set
        {
            if (value != findByStrategy)
            {
                findByStrategy = value;
                if (AvilableIdentifiers?.Any(a => a.AttributeName.Equals(value)) ?? false)
                {
                    Identifier = AvilableIdentifiers.First(a => a.AttributeName.Equals(value)).AttributeValue;
                }
            }
        }
    }

    /// <summary>
    /// Identifier value used to search for the control i.e. name of control if FindByStrategy is name,etc.
    /// </summary>
    [DataMember(IsRequired = true, Order = 220)]
    [Display(Name = "Identifier", Order = 20, GroupName = "Search Strategy")]
    [Description("Identifier value used to search for the control")]
    public string Identifier { get; set; }

    /// <summary>
    /// Wait timeout in seconds for control lookup
    /// </summary>
    [DataMember(Order = 230)]
    [Display(Name = "Search Timout", Order = 40, GroupName = "Search Strategy")]
    [Description("Wait timeout in seconds for control lookup")]
    public int SearchTimeout { get; set; } = 5;

    /// <summary>
    /// Indicates the SearchScope for control lookup e.g. if the control should be looked in child subtree or descendant subtree of the search root, etc.
    /// </summary>
    [DataMember(Order = 240)]
    [Display(Name = "Search Scope", Order = 30, GroupName = "Search Strategy")]
    public override SearchScope SearchScope { get; set; } = SearchScope.Descendants;

    /// <summary>
    /// Holds all the identifiers captured at design time
    /// </summary>
    [DataMember(Order = 250)]
    [Browsable(false)]
    public List<ControlIdentifier> AvilableIdentifiers = new List<ControlIdentifier>();

    #endregion Search Strategy

    /// <summary>
    /// Default constructor
    /// </summary>
    public AppiumNativeControlIdentity() : base()
    {
        RetryAttempts = 2;
    }
   
    /// </inheritdoc>   
    public override string ToString()
    {
        return $"{this.Name} -> FindBy:{this.findByStrategy}|Identifier:{this.Identifier}|SearchScope:{this.SearchScope}";
    }
}