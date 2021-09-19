using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Serilog;
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

        private readonly ILogger logger = Log.ForContext<EntityManager>();

        private IServiceResolver serviceProvider;
        private IFileSystem fileSystem;
        private IArgumentProcessor argumentProcessor;
        private IScriptEngine scriptEngine;
        private bool areDefaultServicesInitialized = false;
        private bool isPrimaryEntityManager = true;
        private string identifier;

        private Entity rootEntity;
        public Entity RootEntity
        {
            get => rootEntity;
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("Null value can't be assigned to RootEntity");
                }
                rootEntity = value;
                if(isPrimaryEntityManager)
                {
                    rootEntity.EntityManager = this;
                    logger.Information($"Root entity set to {rootEntity} for {this}");
                }
            }
        }

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
                if (this.arguments != value)
                {
                    if (!areDefaultServicesInitialized)
                    {
                        this.arguments = value;
                        ConfigureServices();
                        areDefaultServicesInitialized = true;
                    }
                    else
                    {
                        UpdateServices(value, this.arguments);
                        this.arguments = value;
                    }
                    this.arguments = value;
                    UpdateArgumentPropertiesInfo();
                }
            }
        }

        #endregion data members        

        #region Constructor      

        public EntityManager(IServiceResolver serviceProvider)
        {
            this.identifier = "Root";
            this.serviceProvider = serviceProvider;
            logger.Information("Created a new instance of primary entity manager");           
        }

        /// <summary>
        /// Create EntityManager from an existing entity manager 
        /// </summary>
        /// <param name="entityManager"></param>
        public EntityManager(IEntityManager parentEntityManager)
        {
            this.serviceProvider = parentEntityManager.GetServiceOfType<IServiceResolver>();
            this.isPrimaryEntityManager = false;
            this.RootEntity = parentEntityManager.RootEntity;         
            this.SetCurrentFileSystem(parentEntityManager.GetCurrentFileSystem());
            logger.Information("Created a new instance of secondary entity manager");
        }

        #endregion Constructor

        #region Run time services

        public void SetIdentifier(string identifier)
        {
            if(!string.IsNullOrEmpty(identifier))
            {
                this.identifier = identifier;
                logger.Information($"Entity manager identifer set to {identifier}");
            }
        }

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
            return this.argumentProcessor;
        }

        public IScriptEngine GetScriptEngine()
        {
            return this.scriptEngine;
        }

        private void ConfigureServices()
        {
            IScriptEngineFactory scriptEngineFactory = this.GetServiceOfType<IScriptEngineFactory>();

            //For primary entity manager, add the assemblies.For secondary entity manager, data model assembly is same as primary entity manager data model assembly.
            //For prefab entity manager which are also secondary entity manager, data model assembly is loaded using #r in mapping scripts. No need to add them to
            //script engine factory here.
            if (isPrimaryEntityManager)
            {
                var assemblyReferences = fileSystem.ReferenceManager.GetScriptRunTimeReferences();
                scriptEngineFactory = scriptEngineFactory.WithSearchPaths(Environment.CurrentDirectory, Environment.CurrentDirectory, fileSystem.ReferencesDirectory).
                WithAdditionalAssemblyReferences(assemblyReferences);
                scriptEngineFactory = scriptEngineFactory.WithAdditionalAssemblyReferences(this.arguments.GetType().Assembly);
            }



            this.scriptEngine = scriptEngineFactory.CreateScriptEngine(fileSystem.WorkingDirectory);
            this.scriptEngine.SetGlobals(this.arguments);

            this.argumentProcessor = this.GetServiceOfType<IArgumentProcessor>();
            this.argumentProcessor.Initialize(this.scriptEngine, this.arguments);         
        }

        private void UpdateServices(object newArgs, object prevArgs)
        {
            IScriptEngineFactory scriptEngineFactory = this.GetServiceOfType<IScriptEngineFactory>();

            if(isPrimaryEntityManager)
            {
                var previousDataModelAssembly = prevArgs.GetType().Assembly;
                var newDataModelAssembly = newArgs.GetType().Assembly;

                //Note : Important check . Otherwise all script engine states are cleared which is not desirable during test execution
                if (previousDataModelAssembly != newDataModelAssembly)
                {
                    scriptEngineFactory.RemoveReferences(previousDataModelAssembly);
                    scriptEngineFactory.WithAdditionalAssemblyReferences(newDataModelAssembly);
                }
            }

            this.scriptEngine.SetGlobals(newArgs);
            this.argumentProcessor.Initialize(this.scriptEngine, newArgs);

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

        #region Get Component Helpers

        public bool TryGetOwnerApplication(IComponent component, out IApplication application)
        {
            try
            {
                application = GetOwnerApplication(component);
                return true;
            }
            catch
            {
                application = null;
            }
            return false;
        }

        public bool TryGetOwnerApplication<T>(IComponent component, out T application) where T : class, IApplication
        {
            try
            {
                application = GetOwnerApplication<T>(component);
                return true;
            }
            catch
            {
                application = default(T);
            }
            return false;
        }


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
        public IApplicationEntity GetApplicationEntity(IComponent component)
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
            if(current is IApplicationContext applicationContext)
            {
                string targetAppId = applicationContext.GetAppContext();
                return GetApplicationEntity(targetAppId);
            }
            throw new ConfigurationException($"{component} doesn't have an Application Context");

        }

        /// <summary>
        /// Retrieve the application entity with a given application id
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        private IApplicationEntity GetApplicationEntity(string applicationId)
        {
            var applicationPoolEntity = this.RootEntity.GetComponentsByTag("ApplicationPoolEntity", Enums.SearchScope.Children).FirstOrDefault() as Entity;
            var applicationsInPool = applicationPoolEntity.GetComponentsOfType<IApplicationEntity>(Enums.SearchScope.Descendants);
            if (applicationsInPool != null)
            {
                var targetApp = applicationsInPool.FirstOrDefault(a => a.ApplicationId.Equals(applicationId));
                if (targetApp != null)
                {
                    return targetApp;
                }
            }
            throw new ArgumentException($"ApplicationEntity for application with id : {applicationId} is missing from ApplicationPoolentity");
        }

        #endregion  Get Component Helpers

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
            var declaredVariables = this.scriptEngine.GetScriptVariables()?.Where(v => v.PropertyType.Equals(propertyType))?.Select(a => a.PropertyName) ?? Enumerable.Empty<string>();
            matchingProperties.AddRange(declaredVariables);
            return matchingProperties;
        }

        public void RestoreParentChildRelation(IComponent entityComponent)
        {
            if (entityComponent is Entity entity)
            {
                foreach (var component in entity.Components)
                {
                    component.Parent = entity;
                    //There are primary and secondary entity managers. We don't want to overwrite secondary entity managers
                    //when doing a restore from top down.
                    if (component.EntityManager == null)
                    {
                        component.EntityManager = entity.EntityManager;
                    }
                    if (component is Entity childEntity)
                    {
                        RestoreParentChildRelation(childEntity);
                    }
                    Debug.Assert(component.Parent != null);
                    Debug.Assert(component.EntityManager != null);
                }
            }
        }

        #endregion private methods

        public override string ToString()
        {
            return $"Entity Manager - {this.identifier ?? string.Empty} | IsPrimary - {this.isPrimaryEntityManager}";
        }

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

                this.rootEntity = null;
                this.arguments = null;
                this.serviceProvider = null;
                this.fileSystem = null;
            }
            logger.Information($"Entity manager : {this} has been disposed");
        }

        #endregion IDisposable
    }
}
