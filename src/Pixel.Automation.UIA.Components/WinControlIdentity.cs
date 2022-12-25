extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.UIA.Components.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components
{
    [DataContract]
    [Serializable]
    [ContainerEntity(typeof(WinControlEntity))]
    public class WinControlIdentity : ControlIdentity
    {
        [DataMember(Order = 210)]
        [Category("Accessibility")]
        public string AccessKey { get; set; }

        [DataMember(Order = 220)]
        [Category("Accessibility")]
        public string HelpText { get; set; }

        [DataMember(Order = 230)]
        [Category("Accessibility")]
        public string AcceleratorKey { get; set; }

        [DataMember(Order = 240)]
        [Category("Identification")]
        public string AutomationId { get; set; }

        [DataMember(Order = 250)]
        [Category("Identification")]
        public string NameProperty { get; set; }

        [DataMember(Order = 260)]
        [Category("Identification")]   
        [Display(Name = "Control Type")]
        public WinControlType WinControlType { get; set; }

        [DataMember(Order = 270)]
        [Category("Identification")]
        public string ClassName { get; set; }


        [DataMember(Order = 280)]
        [Category("Identification")]
        public bool IsContentElement { get; set; }

        [DataMember(Order = 290)]
        [Category("Identification")]
        public bool IsControlElement { get; set; }

        [DataMember(Order = 300)]
        [Browsable(false)]
        public int Depth { get; set; }


        [DataMember(Order = 310)]
        [Category("Supported Patterns")]
        public List<string> SupportedPatterns { get; set; } = new();


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

        #endregion required during scraping

        public WinControlIdentity() : base()
        {
           
        }


        public override object Clone()
        {
            WinControlIdentity clone = new WinControlIdentity()
            {
                Name = this.Name,     
                Index = this.Index,
                Depth = this.Depth,             
                ApplicationId = this.ApplicationId,                
                PivotPoint = this.PivotPoint,
                XOffSet = this.XOffSet,
                YOffSet = this.YOffSet,               
                RetryAttempts = this.RetryAttempts,     
                RetryInterval = this.RetryInterval,
                AcceleratorKey = this.AcceleratorKey,
                AccessKey = this.AccessKey,
                AutomationId = this.AutomationId,              
                ClassName = this.ClassName,
                WinControlType = this.WinControlType,
                HelpText = this.HelpText,              
                IsContentElement = this.IsContentElement,
                IsControlElement = this.IsControlElement,
                NameProperty = this.NameProperty,               
                SearchScope = this.SearchScope,                     
                Next = this.Next?.Clone() as WinControlIdentity

            };
            clone.SupportedPatterns.AddRange(this.SupportedPatterns);
            return clone;
        }

        public override string ToString()
        {
            return $"{this.Name} -> Name:{this.NameProperty}|AutomationId:{this.AutomationId}|ClassName:{this.ClassName}|ControlType:{this.WinControlType}|SearchScope:{this.SearchScope}";
        }
    }
}

