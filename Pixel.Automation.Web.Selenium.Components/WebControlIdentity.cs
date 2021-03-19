using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Enums;
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
    [ContainerEntity(typeof(WebControlEntity))]
    public class WebControlIdentity : ControlIdentity
    {      
        #region Search Strategy

        string findByStrategy;
        /// <summary>
        ///  FindBy strategy used to search for a control e.g. id, name, css3selector, etc.     
        /// </summary>
        [DataMember(IsRequired = true)]
        [Display(Name = "Find By", Order = 10, GroupName = "Search Strategy")]
        [Description(" FindBy strategy used to search for a control.For ex: id,name,css3selector,etc.")]       
        public virtual string FindByStrategy
        {
            get
            {
                return findByStrategy;
            }
            set
            {
                if(value != findByStrategy)
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
        [DataMember(IsRequired = true)]
        [Display(Name = "Identifier", Order = 20, GroupName = "Search Strategy")]
        [Description("Identifier value used to search for the control")]
        public string Identifier { get; set; }

        /// <summary>
        /// Wait timeout in seconds for control lookup
        /// </summary>
        [DataMember()]
        [Display(Name = "Search Timout", Order = 40, GroupName = "Search Strategy")]
        [Description("Wait timeout in seconds for control lookup")]
        public int SearchTimeout { get; set; } = 5;
        
        /// <summary>
        /// Indicates the SearchScope for control lookup e.g. if the control should be looked in child subtree or descendant subtree of the search root, etc.
        /// </summary>
        [DataMember]
        [Display(Name = "Search Scope", Order = 30, GroupName = "Search Strategy")]   
        public override SearchScope SearchScope { get; set; } = SearchScope.Descendants;

        /// <summary>
        /// Holds all the identifiers captured at design time
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public List<ControlIdentifier> AvilableIdentifiers = new List<ControlIdentifier>();

        #endregion Search Strategy

        #region Frame Details

        /// <summary>
        /// Identification details for the frame that contains the control 
        /// </summary>
        [DataMember(IsRequired = true)]
        [Display(Name = "Frame Hierarchy", Order = 10, GroupName = "Frame Details")]
        [Description("Details of frame hierarchy in web page  which contains the element")]
        public List<FrameIdentity> FrameHierarchy { get; set; } = new List<FrameIdentity>();
     

        #endregion Frame Details

        /// <summary>
        /// Default constructor
        /// </summary>
        public WebControlIdentity() : base()
        {
            RetryAttempts = 2;            
        }

        public override object Clone()
        {
            WebControlIdentity clone = new WebControlIdentity()
            {
                Name = this.Name,     
                Index = this.Index,
                ControlType = this.ControlType,
                ApplicationId = this.ApplicationId,
                ControlImage = this.ControlImage,
                BoundingBox = this.BoundingBox,              
                PivotPoint = this.PivotPoint,
                XOffSet = this.XOffSet,
                YOffSet = this.YOffSet,
                FindByStrategy = this.findByStrategy,               
                Identifier =  this.Identifier,
                RetryAttempts = this.RetryAttempts,
                RetryInterval = this.RetryInterval,
                SearchTimeout = this.SearchTimeout,
                SearchScope = this.SearchScope,
                AvilableIdentifiers = new List<ControlIdentifier>(this.AvilableIdentifiers),
                FrameHierarchy = new List<FrameIdentity>(this.FrameHierarchy ?? new List<FrameIdentity>()),
                Next = this.Next?.Clone() as WebControlIdentity

            };
            return clone;
        }
     
        public override string ToString()
        {
            return $"{this.Name} -> FindBy:{this.findByStrategy}|Identifier:{this.Identifier}|LookUpType:{this.ControlType}|SearchScope:{this.SearchScope}";
        }      
    }
}
