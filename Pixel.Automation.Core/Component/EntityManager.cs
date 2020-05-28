using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Exceptions;
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
    public class EntityManager : IDisposable, IEntityManager
    {
        #region data members       

        private readonly IServiceResolver serviceProvider;
        private IFileSystem fileSystem;
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

        #endregion data members        

        #region Constructor      

        public EntityManager(IServiceResolver serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.serviceProvider.RegisterDefault<IEntityManager>(this);
        }

        /// <summary>
        /// Create EntityManager from an existing entity manager 
        /// </summary>
        /// <param name="entityManager"></param>
        public EntityManager(IEntityManager entityManager, object dataModel)
        {
            //cloning service resolver allows us to register new defaults in scope of this EntityManager since we get a new ninject childkernel
            this.serviceProvider = entityManager.GetServiceOfType<IServiceResolver>().Clone() as IServiceResolver;
            this.RootEntity = entityManager.RootEntity;
            if (dataModel != null)
            {
                this.Arguments = dataModel;
            }
        }

        #endregion Constructor

        #region Run time services

        public void SetCurrentFileSystem(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public IFileSystem GetCurrentFileSystem()
        {
            return this.fileSystem;
        }

        public IArgumentProcessor GetArgumentProcessor()
        {
            return GetServiceOfType<IArgumentProcessor>();
        }

        public IScriptEngine GetScriptEngine()
        {
            return GetServiceOfType<IScriptEngine>();
        }

        #endregion Run time services    
               
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
        }

        /// <summary>
        /// Get all service of a given type from underlying service provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAllServicesOfType<T>()
        {
            return this.serviceProvider.GetAll<T>();            
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

        /// <summary>
        /// Get the owner application for a given component
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public IApplication GetOwnerApplication(IComponent component)
        {
            //This will allow IControlLocator<T> or ICoordinate provider to get IApplication since they are immediate child of ApplicationDetailsEntity
            if (component.Parent is IApplicationEntity appEntity)
            {
                return appEntity.GetTargetApplicationDetails();
            }
            var targetApp = GetApplicationEntity(component);
            return targetApp.GetTargetApplicationDetails();

        }

        /// <summary>
        /// Get owner application of specified type T for a given component 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public T GetOwnerApplication<T>(IComponent component) where T : class, IApplication
        {
            if (component.Parent is IApplicationEntity appEntity)
            {
                return appEntity.GetTargetApplicationDetails<T>();
            }
            var targetApp = GetApplicationEntity(component);
            return targetApp.GetTargetApplicationDetails<T>();
        }


        /// <summary>
        /// Get IControlLocator for a given control identity
        /// </summary>
        /// <param name="forControl"></param>
        /// <returns></returns>
        public IControlLocator GetControlLocator(IControlIdentity forControl)
        {
            var applicationEntity = GetApplicationEntity(forControl.ApplicationId);
            var controlLocator = (applicationEntity as Entity).GetComponentsOfType<IControlLocator>().Single(c => c.CanProcessControlOfType(forControl));
            return controlLocator;
        }

        /// <summary>
        /// Get ICoordinateProvider for a given control identity
        /// </summary>
        /// <param name="forControl"></param>
        /// <returns></returns>
        public ICoordinateProvider GetCoordinateProvider(IControlIdentity forControl)
        {
            var applicationEntity = GetApplicationEntity(forControl.ApplicationId);
            var coordinateProvider = (applicationEntity as Entity).GetComponentsOfType<ICoordinateProvider>().Single(c => c.CanProcessControlOfType(forControl));
            return coordinateProvider;
        }

        /// <summary>
        /// Retrieve the owner application entity for a given component
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private IApplicationEntity GetApplicationEntity(IComponent component)
        {
            var current = component;
            while (true)
            {
                if (current is IApplicationContext)
                {
                    break;
                }
                current = current.Parent;
            }
            string targetAppId = (current as IApplicationContext).GetAppContext();

            return GetApplicationEntity(targetAppId);
        }

        /// <summary>
        /// Retrieve the application entity with a given application id
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        private IApplicationEntity GetApplicationEntity(string applicationId)
        {
            var applicationsInPool = this.RootEntity.GetComponentsOfType<IApplicationEntity>(Enums.SearchScope.Descendants);
            if (applicationsInPool != null)
            {
                var targetApp = applicationsInPool.FirstOrDefault(a => a.ApplicationId.Equals(applicationId));
                if (targetApp != null)
                {
                    return targetApp;
                }
            }

            throw new ConfigurationException($"ApplicationEntity for application with id : {applicationId} is missing from ApplicationPoolentity");
        }       

        #region private methods

        private Dictionary<string, IEnumerable<string>> argumentPropertiesInfo = new Dictionary<string, IEnumerable<string>>();

        /// <summary>
        /// Update cache of all properties grouped by type which are present in arguments object
        /// </summary>
        private void UpdateArgumentPropertiesInfo()
        {
            this.argumentPropertiesInfo.Clear();

            if (this.arguments != null)
            {
                var propertiesGroupedByType = this.arguments.GetType().GetProperties().GroupBy(p => p.PropertyType);
                foreach (var propertyGroup in propertiesGroupedByType)
                {
                    this.argumentPropertiesInfo.Add(propertyGroup.Key.GetDisplayName(), propertyGroup.Select(p => p.Name));
                }
            }
        }

        /// <summary>
        /// Get all the properties of a given type present in either the arguments object or script engine variables collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<string> GetPropertiesOfType(Type propertyType)
        {
            List<string> matchingProperties = new List<string>();

            //look in to arguments object properties
            if (this.argumentPropertiesInfo.ContainsKey(propertyType.GetDisplayName()))
            {
                matchingProperties.AddRange(this.argumentPropertiesInfo[propertyType.GetDisplayName()] ?? Enumerable.Empty<string>());
            }

            //look in to script engine variables
            IScriptEngine scriptEngine = GetServiceOfType<IScriptEngine>();
            var declaredVariables = scriptEngine.GetScriptVariables()?.Where(v => v.PropertyType.Equals(propertyType))?.Select(a => a.PropertyName) ?? Enumerable.Empty<string>();
            matchingProperties.AddRange(declaredVariables);
            return matchingProperties;
        }

        public void RestoreParentChildRelation(IComponent entityComponent, bool resetId = false)
        {
            if (entityComponent is Entity entity)
            {
                foreach (var component in entity.Components)
                {
                    component.Parent = entity;
                    component.EntityManager = entity.EntityManager;
                    if (component is Entity childEntity)
                    {
                        RestoreParentChildRelation(childEntity, resetId);
                    }
                    Debug.Assert(component.Parent != null);
                }
            }
        }

        #endregion private methods

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                foreach (var component in this.RootEntity.GetAllComponents())
                {
                    if (component is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                this.serviceProvider.Dispose();


                RootEntity = null;
                arguments = null;
            }
        }

        #endregion IDisposable
    }
}
