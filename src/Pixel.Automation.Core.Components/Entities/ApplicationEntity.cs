using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components
{

    [DataContract]
    [Serializable]
    public abstract class ApplicationEntity : Entity, IApplicationEntity
    {
        protected readonly ILogger logger;

        [DataMember]
        [Display(Name = "Application Id", Order = 10, GroupName = "Application Details")]
        public string ApplicationId { get; set; }

        [DataMember]
        [Browsable(false)]
        public string ApplicationFile { get; set; }

       
        protected IApplication applicationDetails;


        internal ApplicationEntity()
        {
            this.logger = Log.ForContext(this.GetType());
        }

        public ApplicationEntity(string name = "Application Entity", string tag = "ApplicationEntity") : base(name, tag)
        {
            this.logger = Log.ForContext(this.GetType());
        }

        public virtual IApplication GetTargetApplicationDetails()
        {
            LoadApplicationDetails();
            return this.applicationDetails;
        }

        public virtual T GetTargetApplicationDetails<T>() where T : class, IApplication
        {
            return GetTargetApplicationDetails() as T;
        }

        /// <summary>
        /// Set IApplicationDetails for the ApplicationEntity. This is required at design time when IApplicationDetails points to a live application
        /// and we may want to keep that state e.g. when reloading the automation process because the data model changed.
        /// </summary>
        /// <param name="applicationDetails"></param>
        public void SetTargetApplicationDetails(IApplication applicationDetails)
        {
            this.applicationDetails = applicationDetails;
        }

        /// <summary>
        /// Load the <see cref="ApplicationDescription"/> file which contains all the details for application
        /// </summary>
        protected virtual void LoadApplicationDetails()
        {
            if (this.applicationDetails == null)
            {
                var fileSystem = this.EntityManager.GetCurrentFileSystem();
                if (fileSystem.Exists(this.ApplicationFile))
                {
                    ISerializer serializer = this.EntityManager.GetServiceOfType<ISerializer>();
                    var masterApplicationDetails = serializer.Deserialize<ApplicationDescription>(this.ApplicationFile);
                    this.applicationDetails = masterApplicationDetails.ApplicationDetails;
                    this.Name = this.applicationDetails.ApplicationName;
                    logger.Information("Loaded application details for Application Entity with Id: {0}", this.Id);
                    return;
                }
                throw new FileNotFoundException($"Application file : {this.ApplicationId} doesn't exist.");
            }

        }
     
        /// <summary>
        /// Reload the Appplication Details. This is required at design time when you modify some details 
        /// on Application from the Application Explorer and would want to reload modified data.
        /// </summary>
        public void Reload()
        {
            this.applicationDetails = null;
            LoadApplicationDetails();
        }

        [Browsable(false)]
        ///<inheritdoc/>
        public virtual bool CanUseExisting => false;

        ///<inheritdoc/>
        public abstract void Launch();

        ///<inheritdoc/>
        public abstract void Close();      

        ///<inheritdoc/>
        public virtual void UseExisting(ApplicationProcess application)
        {
            throw new NotSupportedException($"{this.GetType().Name} doesn't support attaching to an already running process.");
        }


        /// <summary>
        /// Add the default control locator for this Application type as a child component whenever ApplicationEntity is added to the AutomationProcess
        /// </summary>
        public override void ResolveDependencies()
        {
            if (!this.Components.Any(a => a is IControlLocator))
            {
                LoadApplicationDetails();
                var controlLocatorAttribute = this.applicationDetails.GetType().GetCustomAttributes(typeof(ControlLocatorAttribute), false).OfType<ControlLocatorAttribute>().FirstOrDefault();
                if (controlLocatorAttribute != null)
                {
                    Component controlLocator = Activator.CreateInstance(controlLocatorAttribute.LocatorType) as Component;
                    if (controlLocator != null)
                    {
                        base.AddComponent(controlLocator);
                        logger.Information("Added control locator of type : {0} to Applicaton Entity " +
                            "with Id : {1}", controlLocatorAttribute.LocatorType, this.Id);
                    }
                }
            }
        }

        #region IDisposable

        /// <summary>
        /// Dispose the Application instance and Close the TargetApplication if not already exited.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                Close();
            }
        }

        #endregion IDisposable
    }
}
