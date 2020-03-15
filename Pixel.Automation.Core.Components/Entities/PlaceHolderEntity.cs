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
        public Type AllowedComponentsType { get; set; } = typeof(Component);

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
                return this;
            }
          
            if(this.MaxComponentsCount.HasValue)
            {              
                if (this.Components.Count < MaxComponentsCount)
                {
                    return base.AddComponent(component);
                }
            }           

            return this;
        }

    }
}
