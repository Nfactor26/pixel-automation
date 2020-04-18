using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core
{
    [DataContract]
    [Serializable]
    public class EntityManager : IDisposable
    {
        #region data members       

        private readonly IServiceResolver serviceProvider;
        private IFileSystem fileSystem;
        private bool isPrimaryManager = false;
        private bool areDefaultServicesInitialized = false;
       
        public Entity RootEntity { get; set; }

        [NonSerialized]
        object arguments;
        public object Arguments
        {
            get
            {
                return this.arguments;
            }
            set
            {
                if (!areDefaultServicesInitialized)
                {
                    this.serviceProvider.ConfigureDefaultServices(this.fileSystem, value.ToScriptArguments(this));
                    areDefaultServicesInitialized = true;
                }
                else
                {
                    IFileSystem fileSystem = this.GetServiceOfType<IFileSystem>();
                    this.serviceProvider.OnDataModelUpdated(fileSystem, this.arguments?.ToScriptArguments(this), value?.ToScriptArguments(this));
                }          
                this.arguments = value;
                UpdateArgumentPropertiesInfo();          
            }
        }
      
        /// <summary>
        /// Working directory represents the process folder
        /// </summary>
        [DataMember]
        public string WorkingDirectory { get; set; }

        #endregion data members        

        #region Constructor      

        public EntityManager(IServiceResolver serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.isPrimaryManager = true;
            this.serviceProvider.RegisterDefault<EntityManager>(this);
        }

        /// <summary>
        /// Create EntityManager from an existing entity manager 
        /// </summary>
        /// <param name="entityManager"></param>
        public EntityManager(EntityManager entityManager, object dataModel)
        {
            //cloning service resolver allows us to register new defaults in scope of this EntityManager since we get a new ninject childkernel
            this.serviceProvider = entityManager.serviceProvider.Clone() as IServiceResolver;          
            this.RootEntity = entityManager.RootEntity;
            if(dataModel != null)
            {
                this.Arguments = dataModel;
            }
            this.isPrimaryManager = false;
        }

        #endregion Constructor

        #region File System

        public void SetCurrentFileSystem(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public IFileSystem GetCurrentFileSystem()
        {
            return this.fileSystem;
        }

        #endregion File System

        #region Find Entities   

        public IComponent FindComponentWithId(string id,SearchScope searchScope=SearchScope.Children)
        {
            return RootEntity.GetComponentById(id, searchScope);
        }
       
        public IEnumerable<IComponent> FindComponentsWithTag(string tag, SearchScope searchScope = SearchScope.Children)
        {
            return RootEntity.GetComponentsByTag(tag, searchScope);
        }


        #endregion Find Entities       

        #region Services     

        /// <summary>
        /// Get service of given type from underlying IServiceProvider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T GetServiceOfType<T>(string key = null)
        {
            return this.serviceProvider.Get<T>(key);
            throw new ArgumentException($"Service of type : {typeof(T)} is not registered with key : {key ?? string.Empty}");
        }

        /// <summary>
        /// Get all service of a given type from underlying service provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAllServicesOfType<T>()
        {
            return this.serviceProvider.GetAll<T>();
            throw new ArgumentException($"Services of type : {typeof(T)} is not registered");
        }
        
        /// <summary>
        ///  Register a instance for a given service type. Whenever this service type is requested, this instance will be returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        public virtual void RegisterDefault<T>(T instance) where T : class
        {
            this.serviceProvider.RegisterDefault<T>(instance);
        }

        #endregion Services      


        private readonly Dictionary<string, IArgumentProcessor> argumentProcessors = new Dictionary<string, IArgumentProcessor>();

        public IArgumentProcessor GetArgumentProcessor(IScopedEntity scopedEntity)
        {
            //if (scopedEntity != null)
            //{
            //    object scopedGlobals = scopedEntity.GetScopedTypeInstance();
            //    if (argumentProcessors.ContainsKey((scopedEntity as IComponent).Id))
            //    {
            //        var cachedArgumentProcessor = argumentProcessors[(scopedEntity as IComponent).Id];
            //        cachedArgumentProcessor.SetGlobals(scopedGlobals);
            //        return cachedArgumentProcessor;
            //    }

            //    IArgumentProcessor scopedArgumentProcessor = GetServiceOfType<IArgumentProcessor>();
            //    scopedArgumentProcessor.SetGlobals(scopedGlobals);
            //    argumentProcessors.Add((scopedEntity as IComponent).Id, scopedArgumentProcessor);
            //    return scopedArgumentProcessor;
            //}

            //if (this.argumentsProcessor == null)
            //{
            //    argumentsProcessor = GetServiceOfType<IArgumentProcessor>();
            //    argumentsProcessor.SetGlobals(this.Arguments);
            //}
            return GetServiceOfType<IArgumentProcessor>();
        }


        private Dictionary<string, IEnumerable<string>> argumentPropertiesInfo = new Dictionary<string, IEnumerable<string>>();

        private void UpdateArgumentPropertiesInfo()
        {
            this.argumentPropertiesInfo.Clear();

            if(this.arguments != null)
            {
                var propertiesGroupedByType = this.arguments.GetType().GetProperties().GroupBy(p => p.PropertyType);
                foreach (var propertyGroup in propertiesGroupedByType)
                {
                    this.argumentPropertiesInfo.Add(propertyGroup.Key.GetDisplayName(), propertyGroup.Select(p => p.Name));
                }
            }            
        }

        /// <summary>
        /// Get all the properties of a given type defined in Argument type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<string> GetPropertiesOfType(Type propertyType)
        {
            List<string> matchingProperties = new List<string>();
            if(this.argumentPropertiesInfo.ContainsKey(propertyType.GetDisplayName()))
            {
                matchingProperties.AddRange(this.argumentPropertiesInfo[propertyType.GetDisplayName()] ?? Enumerable.Empty<string>());
            }
            IScriptEngine scriptEngine = GetServiceOfType<IScriptEngine>();
            var declaredVariables = scriptEngine.GetScriptVariables()?.Where(v => v.PropertyType.Equals(propertyType))?.Select( a => a.PropertyName) ?? Enumerable.Empty<string>();
            matchingProperties.AddRange(declaredVariables);
            return matchingProperties;
        }

        public void RestoreParentChildRelation(IComponent entityComponent, bool resetId = false)
        {   
            if(entityComponent is Entity entity)
            {
                foreach (var component in entity.Components)
                {
                    component.Parent = entity;
                    (component as Component).EntityManager = entity.EntityManager;
                    if (component is Entity)
                    {
                        RestoreParentChildRelation(component as Entity, resetId);
                    }
                    Debug.Assert(component.Parent != null);
                }
            }            
        }
     
        #region IDisposable

        public void Dispose()
        {
            Dispose(true);   
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                (this.serviceProvider as IDisposable)?.Dispose();
                foreach (var component in this.RootEntity.GetAllComponents())
                {
                    if (component is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                RootEntity = null;
                arguments = null;
            }
        }

        #endregion IDisposable
    }
}
