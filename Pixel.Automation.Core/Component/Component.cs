using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [DataMember(Name = "Id",Order =10)]  
        [ReadOnly(true)] 
        [Display(Name="Id",Order = 10, GroupName = "Component", AutoGenerateField = false)]
        /// <inheritdoc/>
        public virtual string Id
        {
            get
            {
                return id;
            }
            protected set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        string name;
        [DataMember(Name = "Name", Order = 20)]   
        [Display(Name = "Name", Order = 20, GroupName = "Component")]
        /// <inheritdoc/>
        public virtual string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                OnPropertyChanged();
            }
        }

        protected string tag;
        [DataMember(Name = "Tag", Order = 30 , IsRequired =false)]
        [ReadOnly(true)]     
        [Display(Name = "Tag", Order = 30, GroupName = "Component")]
        /// <inheritdoc/>
        public virtual string Tag
        {
            get
            {
                return tag;
            }

            set
            {                
                this.tag = value;
                OnPropertyChanged();
            }
        }

        bool isEnabled;
        [DataMember(Name = "Enabled", Order = 40, IsRequired = false)]       
        [Display(Name = "Enabled", Order = 50, GroupName = "Component", Description = "Indicated whether the component will be processed")]
        /// <inheritdoc/>
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }

        int processOrder=1;
        [DataMember(Name= "ProcessOrder", Order =50,IsRequired =true)] 
        [Display(Name = "Order", Order = 40, GroupName = "Component", Description ="Process order of the component")]
        [ReadOnly(true)]
        /// <inheritdoc/>
        public int ProcessOrder
        {
            get
            {
                return processOrder;
            }

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
            get
            {
                return parent;
            }         
            set
            {
              parent = value;
            }
        }       

        [NonSerialized]
        EntityManager entityManager;
        [Browsable(false)]
        [IgnoreDataMember]     
        public EntityManager EntityManager
        {
            get
            {               
                return this.entityManager;
            }

            set
            {            
                this.entityManager = value;
                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public IArgumentProcessor ArgumentProcessor
        {
            get
            {
                return this.GetArgumentProcessor();
            }
        }


        [NonSerialized]
        protected bool isValid=true;   
        [Browsable(false)]    
        public  bool IsValid
        {
            get
            {
                return isValid;
            }   
            protected set
            {
                isValid = value;
                OnPropertyChanged();
            }         
        }

        protected Component(string name="",string tag="")
        {
            this.Id = Guid.NewGuid().ToString();
            this.Name = string.IsNullOrEmpty(name)? GetType().Name : name;
            this.Tag =  string.IsNullOrEmpty(tag) ? GetType().Name : tag;
            this.IsEnabled = true;
        }

        /// <inheritdoc/>
        public virtual void BeforeProcess()
        {

        }

        /// <inheritdoc/>
        public virtual void OnCompletion()
        {

        }

        public virtual void OnFault(IComponent faultingComponent)
        {
           
        }

        /// <inheritdoc/>
        public virtual bool ValidateComponent()
        {
            //validate all the required components are present
            try
            {
                isValid = true;
                if(string.IsNullOrEmpty(name)|| string.IsNullOrEmpty(tag))
                {
                    isValid = false;
                }

                //if(parent==null||eventAggregator==null||entityManager==null)
                //{
                //    isValid = false;
                //}

                //if (parent == null || entityManager == null)
                //{
                //    isValid = false;
                //}

                //var properties = this.GetType().GetProperties();
                //foreach (var prop in properties)
                //{
                //    var requiredComponentAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Equals(typeof(RequiredComponentAttribute)));
                //    if (requiredComponentAttribute != null)
                //    {
                //        if (prop.GetValue(this) == null)
                //        {
                //            isValid = false;
                //        }
                //    }
                //}
                return isValid;
            }
            finally
            {
                OnPropertyChanged("IsValid");
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
            return $"Component - [Name : {Name}] | [Tag : {Tag}] | [IsEnabled :{IsEnabled}]";
        }
    }

    /// <summary>
    /// A data component contains data that is used by another components. 
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class DataComponent : Component
    {
        protected DataComponent(string name = "", string tag = ""):base(name,tag)
        {
            
        }
    }

    /// <summary>
    /// Actor components contain the processing logic to do some action by consuming data components and service components
    /// </summary>
    [DataContract]
    [Serializable]
    public  abstract class ActorComponent : Component
    {
        /// <summary>
        /// Indicates whether the processor should continue with the next child if this actor was not processed successfully
        /// </summary>
        [DataMember]
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
                if(isExecuting)
                {
                    IsFaulted = false;
                }
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
            get
            {
                return isFaulted;
            }
            set
            {
                isFaulted = value;               
                OnPropertyChanged();
            }
        }

        [NonSerialized]
        private List<string> errorMessages = new List<string>();
        [Browsable(false)]
        [IgnoreDataMember]
        public List<string> ErrorMessages
        {
            get
            {
                return errorMessages;
            }
            set
            {
                errorMessages = value;
                OnPropertyChanged("ErrorMessages");
            }
        }


        protected ActorComponent(string name = "", string tag = ""):base(name,tag)
        {

        }

        /// <summary>
        /// Processor calls Act on its child actor components  
        /// </summary>
        public abstract void Act();

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


    [DataContract]
    [Serializable]
    public abstract class AsyncActorComponent : Component
    {
        /// <summary>
        /// Indicates whether the processor should continue with the next child if this actor was not processed successfully
        /// </summary>
        [DataMember]
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

        /// <summary>
        /// Indicates whether the component is in a faulted state after processing
        /// </summary>
        [NonSerialized]
        private bool isFaulted;

        [Browsable(false)]
        [IgnoreDataMember]
        public bool IsFaulted
        {
            get
            {
                return isFaulted;
            }
            set
            {
                isFaulted = value;
                OnPropertyChanged();
            }
        }

        [NonSerialized]
        private List<string> errorMessages = new List<string>();
        [Browsable(false)]
        [IgnoreDataMember]
        public List<string> ErrorMessages
        {
            get
            {
                return errorMessages;
            }
            set
            {
                errorMessages = value;
                OnPropertyChanged("ErrorMessages");
            }
        }


        protected AsyncActorComponent(string name = "", string tag = "") : base(name, tag)
        {

        }

        /// <summary>
        /// Processor calls Act on its child actor components  
        /// </summary>
        public abstract Task ActAsync();

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
        protected ServiceComponent(string name = "", string tag = ""):base(name,tag)
        {

        }
    }
}
