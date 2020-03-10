using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

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
        
        int controlTypeId;
        [DataMember]
        [Category("Identification")]
        public int ControlTypeId
        {
            get
            {
                return controlTypeId;
            }

            set
            {
                controlTypeId = value;
                OnPropertyChanged("ControlTypeId");
                OnPropertyChanged("ControlType");
            }
        }      

        public string ProgrammaticName
        {
            get
            {
                return System.Windows.Automation.ControlType.LookupById(this.ControlTypeId).ProgrammaticName;
            }           
        }

        [DataMember]
        [Category("Identification")]
        public string ClassName { get; set; }

     
        [DataMember]
        [Category("Identification")]
        public bool IsContentElement { get; set; }

        [DataMember]
        [Category("Identification")]
        public bool IsControlElement { get; set; }


        System.Windows.Rect boundingRectangle;
        [DataMember]
        [Category("Visibility")]
        public System.Windows.Rect BoundingRectangle
        {
            get { return boundingRectangle; }
            set { boundingRectangle = value; }
        }

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

        public WinControlIdentity()
        {
            this.SupportedPatterns = new List<string>();
        }


        public override object Clone()
        {
            WinControlIdentity clone = new WinControlIdentity()
            {
                Name = this.Name,
                Id = Guid.NewGuid().ToString(),
                EntityManager = this.EntityManager,
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
                boundingRectangle = this.boundingRectangle,
                ClassName = this.ClassName,
                controlTypeId = this.controlTypeId,
                HelpText = this.HelpText,              
                IsContentElement = this.IsContentElement,
                IsControlElement = this.IsControlElement,
                NameProperty = this.NameProperty,
                OwnerApplication = this.OwnerApplication,
                SearchScope = this.SearchScope,
                tag = this.tag,            
                Next = this.Next?.Clone() as WinControlIdentity

            };
            return clone;
        }
    }
}

