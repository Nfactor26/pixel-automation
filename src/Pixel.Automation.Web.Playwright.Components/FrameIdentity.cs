using Pixel.Automation.Core;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

[DataContract]
[Serializable]
public class FrameIdentity : NotifyPropertyChanged
{     
    /// <summary>
    /// Identifier used to search for the control i.e. name of control if FindByStrategy is name,etc.
    /// </summary>
    [DataMember(IsRequired = true, Order = 20)]
    [Display(Name = "Identifier", GroupName = "Configuration", Order = 20, Description = "Identifier used to search for the control")]
    public virtual string Identifier
    {
        get;
        set;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public FrameIdentity()
    {
       
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Frame -> Identifier:{this.Identifier}";
    }

}

public class FrameEqualityComparer : IEqualityComparer<FrameIdentity>
{
    public bool Equals(FrameIdentity x, FrameIdentity y)
    {
        if (Object.ReferenceEquals(x, y))
        {
            return true;
        }
        return x.Identifier.Equals(y.Identifier);

    }

    public int GetHashCode(FrameIdentity obj)
    {
        return obj.GetHashCode();
    }
}
