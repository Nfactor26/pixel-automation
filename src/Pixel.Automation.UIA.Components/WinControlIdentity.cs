extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.UIA.Components.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Pixel.Automation.UIA.Components
{
    [DataContract]
    [Serializable]
    [ContainerEntity(typeof(WinControlEntity))]
    public class WinControlIdentity : ControlIdentity
    {
        [DataMember]
        [Category("Accessibility")]
        public string AccessKey { get; set; }

        [DataMember]
        [Category("Accessibility")]
        public string HelpText { get; set; }

        [DataMember]
        [Category("Accessibility")]
        public string AcceleratorKey { get; set; }

        [DataMember]
        [Category("Identification")]
        public string AutomationId { get; set; }

        [DataMember]
        [Category("Identification")]
        public string NameProperty { get; set; }
        
        [DataMember]
        [Category("Identification")]   
        [Display(Name = "Control Type")]
        public WinControlType WinControlType { get; set; }

        [DataMember]
        [Category("Identification")]
        public string ClassName { get; set; }

     
        [DataMember]
        [Category("Identification")]
        public bool IsContentElement { get; set; }

        [DataMember]
        [Category("Identification")]
        public bool IsControlElement { get; set; }
                
        [DataMember]
        [Browsable(false)]
        public int Depth { get; set; }
        

        [DataMember]
        [Category("Supported Patterns")]
        public List<string> SupportedPatterns { get; set; }


        #region required during scraping

        [NonSerialized]
        int processId;
        [Browsable(false)]
        public int ProcessId
        {
            get
            {
                return processId;
            }
            set
            {
                processId = value;
            }
        }

        [DataMember]
        public string OwnerApplication { get; set; }

        #endregion required during scraping

        public WinControlIdentity() : base()
        {
            this.SupportedPatterns = new List<string>();
        }


        public override object Clone()
        {
            WinControlIdentity clone = new WinControlIdentity()
            {
                Name = this.Name,     
                Index = this.Index,
                Depth = this.Depth,
                LookupType = this.LookupType,
                ApplicationId = this.ApplicationId,
                ControlImage = this.ControlImage,
                BoundingBox = this.BoundingBox,
                PivotPoint = this.PivotPoint,
                XOffSet = this.XOffSet,
                YOffSet = this.YOffSet,               
                RetryAttempts = this.RetryAttempts,     
                RetryInterval = this.RetryInterval,
                AcceleratorKey = this.AcceleratorKey,
                AccessKey = this.AccessKey,
                AutomationId = this.AutomationId,
                boundingBox = this.BoundingBox,
                ClassName = this.ClassName,
                WinControlType = this.WinControlType,
                HelpText = this.HelpText,              
                IsContentElement = this.IsContentElement,
                IsControlElement = this.IsControlElement,
                NameProperty = this.NameProperty,
                OwnerApplication = this.OwnerApplication,
                SearchScope = this.SearchScope,                     
                Next = this.Next?.Clone() as WinControlIdentity

            };
            return clone;
        }

        public override string ToString()
        {
            return $"{this.Name} -> Name:{this.NameProperty}|AutomationId:{this.AutomationId}|ClassName:{this.ClassName}|ControlType:{this.WinControlType}|LookUpType:{this.LookupType}|SearchScope:{this.SearchScope}";
        }
    }
}

