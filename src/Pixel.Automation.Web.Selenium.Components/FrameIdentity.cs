using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    public class FrameIdentity : NotifyPropertyChanged
    {

        string findByStrategy;
        /// <summary>
        ///  FindBy strategy used to search for a control.
        ///  For ex : id,name,css3selector,etc.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Display(Name = "Find By", GroupName = "Configuration", Order = 10, Description = "FindBy strategy used to search for frame")]
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
                       Identifier = AvilableIdentifiers.
                                First(a => a.AttributeName.Equals(value)).AttributeValue;
                    }
                }
            }
        }        
       
        /// <summary>
        /// Identifier used to search for the control i.e. name of control if FindByStrategy is name,etc.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Display(Name = "Identifier", GroupName = "Configuration", Order = 20, Description = "Identifier used to search for the control")]
        public virtual string Identifier
        {
            get; set;
        }

        /// <summary>
        /// Holds all the identifiers captured at design time
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public List<ControlIdentifier> AvilableIdentifiers = new List<ControlIdentifier>();

        public FrameIdentity()
        {

        }

        public override string ToString()
        {
            return $"Frame -> FindBy:{this.findByStrategy}|Identifier:{this.Identifier}";
        }

    }

    public class FrameEqualityComparer : IEqualityComparer<FrameIdentity>
    {
        public bool Equals(FrameIdentity x, FrameIdentity y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            return x.FindByStrategy.Equals(y.FindByStrategy) && x.Identifier.Equals(y.Identifier);

        }

        public int GetHashCode(FrameIdentity obj)
        {
            //TODO : Implement non-default hash code.
            return obj.GetHashCode();
        }
    }

    //class FindFrameByItemsSource : IItemsSource
    //{
    //    public ItemCollection GetValues()
    //    {
    //        ItemCollection strategies = new ItemCollection();
    //        strategies.Add("Id");
    //        strategies.Add("Index");
    //        strategies.Add("CssSelector");         
    //        strategies.Add("Name");           
    //        strategies.Add("XPath");
    //        return strategies;
    //    }
    //}
}
