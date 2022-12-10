using Dawn;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls
{
    /// <summary>
    /// ControlDescription is used to store the details of a control
    /// </summary>
    [DataContract]
    [Serializable]
    public class ControlDescription : ICloneable
    {
        /// <summary>
        /// Identifier of the owner application
        /// </summary>
        [DataMember(Order = 10)]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Identifier of the Control
        /// </summary>
        [DataMember(Order = 20)]
        public string ControlId { get; set; }

        /// <summary>
        /// Name of the control
        /// </summary>
        [DataMember(Order = 30)]
        public string ControlName { get; set; }

        /// <summary>
        /// Version of the control
        /// </summary>
        [DataMember(Order = 40)]
        public Version Version { get; set; }

        /// <summary>
        /// Group name to which the control belongs. "Default" is the default group.
        /// </summary>
        [DataMember(Order = 50)]
        public string GroupName { get; set; } = "Default";              
       
        /// <summary>
        /// Image of the control
        /// </summary>
        [DataMember(Order = 60)]
        public string ControlImage { get; set; }      

        /// <summary>
        /// <see cref="IControlIdentity"/> of a control i.e. details that can be used to locate control at runtime
        /// </summary>
        [DataMember(Order = 100)]
        public IControlIdentity ControlDetails { get; set; }

        /// <summary>
        /// Indicates if the Control was deleted.
        /// </summary>
        [DataMember(Order = 1000)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ControlDescription()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controlDetails"></param>
        public ControlDescription(IControlIdentity controlDetails)
        {
            Guard.Argument(controlDetails).NotNull();

            this.ApplicationId = controlDetails.ApplicationId;
            this.ControlId = Guid.NewGuid().ToString();           
            this.ControlDetails = controlDetails;
        }

        /// <summary>
        /// Create a copy of ControlDescription
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new ControlDescription(ControlDetails.Clone() as IControlIdentity)
            {
                ControlName = this.ControlName,              
                ControlImage = this.ControlImage,                         
                GroupName = this.GroupName,
                ApplicationId = this.ApplicationId,
                Version = this.Version
            };
        }

        /// <inheritdoc/>      
        public override string ToString()
        {
            return this.ControlDetails?.ToString();
        }
    }
}
