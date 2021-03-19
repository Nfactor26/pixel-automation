using Pixel.Automation.Core.Interfaces;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components
{
    [DataContract]
    [Serializable]   
    public class PlaceHolderEntity : Entity 
    {
        [DataMember]
        [System.ComponentModel.Browsable(false)]
        public int? MaxComponentsCount { get; set; } = 50;

        [DataMember]
        [System.ComponentModel.Browsable(false)]
        public Type AllowedComponentsType { get; set; } = typeof(IComponent);

        public PlaceHolderEntity() : base("Place Holder","PlaceHolder")
        {
           
        }

        public PlaceHolderEntity(string name,string tag= "PlaceHolder"):base(name, tag)
        {

        }

        public override Entity AddComponent(IComponent component)
        {
            if(!AllowedComponentsType.IsAssignableFrom(component.GetType()))
            {
                throw new ArgumentException($"Only components of type {AllowedComponentsType} can be added");
            }
          
            if(this.MaxComponentsCount.HasValue)
            {              
                if (this.Components.Count < MaxComponentsCount)
                {
                    return base.AddComponent(component);
                }
                else
                {
                    throw new InvalidOperationException($"Allowed capacity of {MaxComponentsCount} already reached. Can't add any more child component to {this}");
                }
            }           

            return this;
        }

    }
}
