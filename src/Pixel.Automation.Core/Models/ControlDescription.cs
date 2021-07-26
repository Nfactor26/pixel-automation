﻿using Pixel.Automation.Core.Interfaces;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public class ControlDescription : NotifyPropertyChanged , ICloneable
    {
        [DataMember]
        public string ApplicationId { get; set; }

        [DataMember]
        public string ControlId { get; set; }

        [DataMember]
        public string ControlName { get; set; }

        string groupName = "Default";
        [DataMember]
        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                groupName = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private string controlImage;
        public string ControlImage
        {
            get => controlImage;
            set
            {
                controlImage = value;
                this.ControlDetails.ControlImage = value;
            }
        }
     
        [DataMember]
        public IControlIdentity ControlDetails { get; set; }


        public ControlDescription()
        {

        }

        public ControlDescription(IControlIdentity controlDetails)
        {          
            this.ApplicationId = controlDetails.ApplicationId;
            this.ControlId = Guid.NewGuid().ToString();           
            this.ControlDetails = controlDetails;
        }

        public object Clone()
        {
            return new ControlDescription(ControlDetails.Clone() as IControlIdentity)
            {
                ControlName = this.ControlName,              
                ControlImage = this.ControlImage,                         
                GroupName = this.GroupName,
                ApplicationId = this.ApplicationId               
            };
        }

        public override string ToString()
        {
            return this.ControlDetails?.ToString();
        }
    }
}
