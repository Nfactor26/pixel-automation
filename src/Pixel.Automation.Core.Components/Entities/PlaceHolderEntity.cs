using Pixel.Automation.Core.Interfaces;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components;

[DataContract]
[Serializable]   
public class PlaceHolderEntity : Entity 
{
    [DataMember(Order = 200)]
    [System.ComponentModel.Browsable(false)]
    public int? MaxComponentsCount { get; set; } = 50;

    /// <summary>
    /// Store an interface name to AllowedComponentsType property.
    /// Only components that implement this interface are allowed to be added to this PlaceHolderEntity
    /// </summary>
    [DataMember(Order = 210)]
    [System.ComponentModel.Browsable(false)]
    public string AllowedComponentsType { get; set; } = typeof(IComponent).Name;

    public PlaceHolderEntity() : base("Place Holder", "PlaceHolder")
    {
       
    }

    public PlaceHolderEntity(string name, string tag= "PlaceHolder"):base(name, tag)
    {

    }

    public override Entity AddComponent(IComponent component)
    {
        if(!this.Components.Contains(component))
        {
            if (component.GetType().GetInterface(AllowedComponentsType) == null)
            {
                throw new ArgumentException($"Only components of type {AllowedComponentsType} can be added");
            }

            if (this.MaxComponentsCount.HasValue)
            {
                if (this.Components.Count < MaxComponentsCount)
                {
                    return base.AddComponent(component);
                }
                throw new InvalidOperationException($"Allowed capacity of {MaxComponentsCount} already reached. Can't add any more child component to {this}");
            }
        }
        return this;
    }

}
