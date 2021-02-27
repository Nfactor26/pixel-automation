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
        ///  FindBy strategy used to search for a control.
        ///  For ex : id,name,css3selector,etc.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Description(" FindBy strategy used to search for a control.For ex: id,name,css3selector,etc.")]    
        [Display(Name = "Find By", Order = 10, GroupName = "Search Strategy")]
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
        /// Identifier used to search for the control i.e. name of control if FindByStrategy is name,etc.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Description("Identifier used to search for the control")]       
        [Display(Name = "Identifier", Order = 20, GroupName = "Search Strategy")]
        public string Identifier
        {
            get;set;
        }

        [DataMember()]
        [Description("Explicit wait timeout in seconds for control during lookup")]
        [Display(Name = "Search Timout", Order = 40, GroupName = "Search Strategy")]
        public int SearchTimeout { get; set; } = 10;

        [DataMember]
        [Display(Name = "Search Scope", Order = 30, GroupName = "Search Strategy")]
        //[ItemsSource(typeof(SearchScopeItemSource))]
        public override SearchScope SearchScope { get; set; } = SearchScope.Descendants;

        /// <summary>
        /// Holds all the identifiers captured at design time
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public List<ControlIdentifier> AvilableIdentifiers = new List<ControlIdentifier>();

        #endregion Search Strategy

        #region Frame Details

        List<FrameIdentity> frameHierarchy = new List<FrameIdentity>();
        /// <summary>
        /// Indicates the index of the frame inside a window/tab where the target control can be located. 
        /// </summary>
        [DataMember(IsRequired = true)]
        [Description("Details of frame hierarchy in web page  which contains the element")]
        [Display(Name = "Frame Hierarchy", Order = 10, GroupName = "Frame Details")]
        public List<FrameIdentity> FrameHierarchy
        {
            get
            {             
                return frameHierarchy;
            }    
            set
            {
                frameHierarchy = value;
            }
        }

        #endregion Frame Details

        public WebControlIdentity() : base()
        {
                         
        }


        public override object Clone()
        {
            WebControlIdentity clone = new WebControlIdentity()
            {
                Name = this.Name,                          
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
                FrameHierarchy = new List<FrameIdentity>(this.FrameHierarchy),
                Next = this.Next?.Clone() as WebControlIdentity

            };
            return clone;
        }
     
        public override string ToString()
        {
            return $"{this.Name} -> FindBy:{this.findByStrategy}|Identifier:{this.Identifier}|LookUpType:{this.ControlType}|SearchScope:{this.SearchScope}";
        }
      
    }

    //class FindControlByItemSource : IItemsSource
    //{
    //    public ItemCollection GetValues()
    //    {
    //        ItemCollection strategies = new ItemCollection();
    //        strategies.Add("Id");
    //        strategies.Add("ClassName");
    //        strategies.Add("CssSelector");
    //        strategies.Add("LinkText");
    //        strategies.Add("Name");
    //        strategies.Add("PartialLinkText");
    //        strategies.Add("TagName");
    //        strategies.Add("XPath");
    //        return strategies;
    //    }
    //}

    //class SearchScopeItemSource : IItemsSource
    //{
    //    public ItemCollection GetValues()
    //    {
    //        ItemCollection strategies = new ItemCollection();
    //        strategies.Add(SearchScope.Ancestor, "Ancestor");
    //        strategies.Add(SearchScope.Descendants,"Descendants");
    //        strategies.Add(SearchScope.Sibling, "Sibling");          
    //        return strategies;
    //    }
    //}
}
