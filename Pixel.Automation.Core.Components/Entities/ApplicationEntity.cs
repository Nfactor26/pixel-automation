using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using Component = Pixel.Automation.Core.Component;

namespace Pixel.Automation.Core.Components
{

    [DataContract]
    [Serializable]
    public class ApplicationEntity : Entity
    {
        [DataMember]
        [Display(Name = "Application Id", Order = 10, GroupName = "Application Details")]
        public string ApplicationId { get; set; }

        [DataMember]
        [Browsable(false)]
        public string ApplicationFile { get; set; }
      
        private IApplication applicationDetails;


        public ApplicationEntity(string name = "Application", string tag = "Application") : base(name,tag)
        {

        }

        public IApplication GetTargetApplicationDetails()
        {
            LoadApplicationDetails();
            return this.applicationDetails;
        }

        public T GetTargetApplicationDetails<T>() where T : class, IApplication
        {
            return GetTargetApplicationDetails() as T;         
        }

        private void LoadApplicationDetails()
        {
            if (this.applicationDetails == null)
            {
                if (File.Exists(this.ApplicationFile))
                {
                    ISerializer serializer = this.EntityManager.GetServiceOfType<ISerializer>();
                    var masterApplicationDetails = serializer.Deserialize<ApplicationDescription>(this.ApplicationFile);
                    this.applicationDetails = masterApplicationDetails.ApplicationDetails;
                    (this.applicationDetails as Component).Parent = this;
                    (this.applicationDetails as Component).EntityManager = this.EntityManager;                  
                    this.Name = this.applicationDetails.ApplicationName;
                    return;
                }
                throw new FileNotFoundException($"Application file : {this.ApplicationId} doesn't exist.");
            }

        }

    }
}
