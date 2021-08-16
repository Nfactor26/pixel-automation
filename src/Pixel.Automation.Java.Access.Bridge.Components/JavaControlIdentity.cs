using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    [DataContract]
    [Serializable]
    [ContainerEntity(typeof(JavaControlEntity))]
    public class JavaControlIdentity : ControlIdentity
    {

        string controlName;
        [DataMember(Order = 210)]
        [Category("Identification")]
        public string ControlName
        {
            get
            {
                return controlName;
            }
            set
            {
                controlName = value;
            }
        }

        string description;
        [DataMember(Order = 220)]
        [Category("Identification")]
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        string role;
        [DataMember(Order = 230)]
        [Category("Identification")]
        public string Role
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
        }

        int depth;
        [DataMember(Order = 240)]
        [Browsable(false)]      
        public int Depth
        {
            get
            {
                return depth;
            }
            set
            {
                depth = value;
            }
        }

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


        string ownerApplication;
        [DataMember]
        public string OwnerApplication
        {
            get
            {
                return ownerApplication;
            }
            set
            {
                ownerApplication = value;
            }
        }

        #endregion required during scraping

        public JavaControlIdentity() : base()
        {

        }

        public override object Clone()
        {
            JavaControlIdentity clone = new JavaControlIdentity()
            {
                Name = this.Name,
                ApplicationId = this.ApplicationId,
                BoundingBox = this.BoundingBox,
                PivotPoint = this.PivotPoint,
                XOffSet = this.XOffSet,
                YOffSet = this.YOffSet,
                ControlName = this.ControlName,
                Description = this.Description,
                Role = this.Role,              
                Depth = this.Depth, 
                Index = this.Index,
                LookupType = this.LookupType,
                OwnerApplication = this.OwnerApplication,
                Next = this.Next?.Clone() as JavaControlIdentity
            };
            return clone;
        }


        public override string ToString()
        {
            return $"{this.Name} -> ControlName:{this.controlName}|Role:{this.Role}|Description:{this.Description}|Depth:{this.Depth}|" +
                $"SearchScope:{this.SearchScope}|Index:{this.Index}";
        }

    }
}
