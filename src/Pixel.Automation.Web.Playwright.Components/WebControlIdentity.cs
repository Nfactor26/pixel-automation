﻿﻿using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components
{
    [DataContract]
    [Serializable]
    [ContainerEntity(typeof(WebControlEntity))]
    public class WebControlIdentity : ControlIdentity
    {
        #region Search Strategy

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

     
        #endregion Search Strategy

        #region Frame Details

        /// <summary>
        /// Identification details for the frame that contains the control 
        /// </summary>
        [DataMember(IsRequired = true, Order = 510)]
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
                LookupType = this.LookupType,
                ApplicationId = this.ApplicationId,              
                PivotPoint = this.PivotPoint,
                XOffSet = this.XOffSet,
                YOffSet = this.YOffSet,             
                Identifier = this.Identifier,
                RetryAttempts = this.RetryAttempts,
                RetryInterval = this.RetryInterval,
                SearchTimeout = this.SearchTimeout,
                SearchScope = this.SearchScope,              
                FrameHierarchy = new List<FrameIdentity>(this.FrameHierarchy ?? new List<FrameIdentity>()),
                Next = this.Next?.Clone() as WebControlIdentity

            };
            return clone;
        }

        public override string ToString()
        {
            return $"{this.Name} -> Identifier:{this.Identifier}|LookUpType:{this.LookupType}|SearchScope:{this.SearchScope}";
        }
    }
}

