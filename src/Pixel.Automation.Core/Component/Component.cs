﻿using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Base class for a component
    /// </summary>
    [DataContract]   
    [Serializable]
    public abstract class Component : NotifyPropertyChanged, IComponent
    {    

        string id;
        [DataMember(Order =10)]  
        [ReadOnly(true)] 
        [Display(Name="Id",Order = 10, GroupName = "Component", AutoGenerateField = false)]
        /// <inheritdoc/>
        public virtual string Id
        {
            get => this.id;
            protected set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        string name;
        [DataMember(Order = 20)]   
        [Display(Name = "Name", Order = 20, GroupName = "Component")]
        /// <inheritdoc/>
        public virtual string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                OnPropertyChanged();
            }
        }

        protected string tag;
        [DataMember(Order = 30 , IsRequired =false)]
        [ReadOnly(true)]     
        [Display(Name = "Tag", Order = 30, GroupName = "Component")]
        /// <inheritdoc/>
        public virtual string Tag
        {
            get => this.tag;
            set
            {
                this.tag = value;
                OnPropertyChanged();
            }
        }

        bool isEnabled;
        [DataMember(Order = 40, IsRequired = false)]       
        [Display(Name = "Enabled", Order = 50, GroupName = "Component", Description = "Indicated whether the component will be processed")]
        /// <inheritdoc/>
        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                this.isEnabled = value;
                OnPropertyChanged();
            }
        }

        int processOrder = 1;
        [DataMember(Order =50, IsRequired =true)] 
        [Display(Name = "Order", Order = 40, GroupName = "Component", Description ="Process order of the component")]
        [ReadOnly(true)]
        /// <inheritdoc/>
        public int ProcessOrder
        {
            get => this.processOrder;
            set
            {
                this.processOrder = value;
                OnPropertyChanged();
            }
        }

        [NonSerialized]
        Entity parent;
        [Browsable(false)]
        [IgnoreDataMember]
        /// <inheritdoc/>
        public Entity Parent
        {
            get => this.parent;
            set
            {
                this.parent = value;
            }
        }

        [NonSerialized]
        IEntityManager entityManager;
        [Browsable(false)]
        [IgnoreDataMember]     
        public IEntityManager EntityManager
        {
            get => this.entityManager;
            set
            {
                this.entityManager = value;
                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public IArgumentProcessor ArgumentProcessor  => this.EntityManager.GetArgumentProcessor();
       
        [NonSerialized]
        protected bool isValid = true;   
        [Browsable(false)]
        public bool IsValid
        {
            get => this.isValid;
            protected set
            {
                this.isValid = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates whether the component is in a faulted state after processing
        /// </summary>
        [NonSerialized]
        private bool isFaulted;

        [Browsable(false)]
        [IgnoreDataMember]
        public bool IsFaulted
        {
            get => this.isFaulted;
            set
            {
                this.isFaulted = value;
                OnPropertyChanged();
            }
        }

        public Component()
        {
            this.Id = Guid.NewGuid().ToString();
            this.IsEnabled = true;
            this.Name = GetType().Name;
            this.Tag = GetType().Name;
        }

        public Component(string name = "", string tag = "")
        {
            this.Id = Guid.NewGuid().ToString();
            this.IsEnabled = true;
            this.Name = string.IsNullOrEmpty(name)? GetType().Name : name;
            this.Tag =  string.IsNullOrEmpty(tag) ? GetType().Name : tag;
            
        }
     
        /// <inheritdoc/>
        public virtual bool ValidateComponent()
        {           
            try
            {
                IsValid = true;
                if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(tag))
                {
                    IsValid = false;
                }              
                return IsValid;
            }
            finally
            {
                OnPropertyChanged(nameof(IsValid));
            }
        }

        /// <inheritdoc/>
        public virtual void ResetComponent()
        {
            
        }   
        
        /// <inheritdoc/>
        public virtual void ResolveDependencies()
        {

        }

        public override string ToString()
        {
            return $"Component -> Name:{Name}|Tag:{Tag}|IsEnabled:{IsEnabled}";
        }
    }

    /// <summary>
    /// A data component contains data that is used by another components. 
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class DataComponent : Component
    {
        public DataComponent() : base()
        {

        }

        public DataComponent(string name = "", string tag = ""):base(name, tag)
        {
            
        }
    }

    [DataContract]
    [Serializable]
    public abstract class ActorComponent : Component
    {
        /// <summary>
        /// Indicates whether the processor should continue with the next child if this actor was not processed successfully
        /// </summary>
        [DataMember(Order = 100)]
        [Display(Name = "Continue on Error", Order = 40, GroupName = "Error Handling", Description = "Indicates whether the processor should ignore any error in this component and " +
            "continue processing next component")]
        public bool ContinueOnError { get; set; } = false;

        /// <summary>
        /// Indicates whether the component is currently being executed by the processor
        /// </summary>
        [NonSerialized]
        private bool isExecuting;

        [Browsable(false)]
        public bool IsExecuting
        {
            get
            {
                return isExecuting;
            }
            set
            {
                isExecuting = value;
                if (isExecuting)
                {
                    IsFaulted = false;
                }
                OnPropertyChanged();
            }

        }        

        [NonSerialized]
        private List<string> errorMessages = new ();
        [Browsable(false)]
        [IgnoreDataMember]
        public List<string> ErrorMessages
        {
            get
            {
                return errorMessages;
            }           
        }
                

        public ActorComponent() : base()
        {

        }

        public ActorComponent(string name = "", string tag = "") : base(name, tag)
        {

        }

        /// <summary>
        /// Processor calls Act on its child actor components  
        /// </summary>
        public abstract Task ActAsync();

        /// <summary>
        /// OnExecutedAsync will be executed after ActAsync is completed
        /// </summary>
        /// <returns></returns>
        public virtual async Task OnCompletionAsync()
        {
            if (TraceManager.IsEnabled)
            {
                await CaptureScreenShotAsync();
            }
        }


        /// <summary>
        /// Capture screenshot of the active page
        /// </summary>
        /// <returns></returns>
        public async Task CaptureScreenShotAsync()
        {
            var ownerApplicationEntity = this.EntityManager.GetApplicationEntity(this);
            if (TraceManager.IsEnabled && ownerApplicationEntity.AllowCaptureScreenshot)
            {               
                string imageFile = GetImagePath();
                await ownerApplicationEntity.CaptureScreenShotAsync(imageFile);
                TraceManager.AddImage(Path.GetFileName(imageFile));
            }

            //We are using EntityManager from owner application to ensure that we are always getting a path based on file system of primary entity manager.
            //Without this, components from prefab will end up getting path in prefab temp directory that can't be located later while trying to upload image.
            string GetImagePath()
            {
                return Path.Combine((ownerApplicationEntity as Entity).EntityManager.GetCurrentFileSystem().TempDirectory, $"{Path.GetRandomFileName()}.jpeg");
            }
        }

        /// <inheritdoc/>
        public override void ResetComponent()
        {
            this.isExecuting = false;
            this.IsFaulted = false;
            this.ErrorMessages.Clear();
        }

        [OnDeserialized]
        public new void Initialize(StreamingContext context)
        {
            this.errorMessages = new List<string>();
        }
    }

    /// <summary>
    /// ServiceComponents are reusable components that provide framework specific services and can be used by ActorComponents.  
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class ServiceComponent : Component
    {
        public ServiceComponent() : base()
        {

        }

        public ServiceComponent(string name = "", string tag = ""):base(name, tag)
        {

        }
    }

}
