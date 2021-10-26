﻿using Dawn;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls
{
    [DataContract]
    [Serializable]
    public class ControlDescription : ICloneable
    {
        [DataMember(Order = 10)]
        public string ApplicationId { get; set; }

        [DataMember(Order = 20)]
        public string ControlId { get; set; }

        [DataMember(Order = 30)]
        public string ControlName { get; set; }

        [DataMember(Order = 40)]
        public string GroupName { get; set; } = "Default";              
       
        [DataMember(Order = 50)]
        public string ControlImage { get; set; }      

        [DataMember(Order = 100)]
        public IControlIdentity ControlDetails { get; set; }


        public ControlDescription()
        {

        }

        public ControlDescription(IControlIdentity controlDetails)
        {
            Guard.Argument(controlDetails).NotNull();

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
