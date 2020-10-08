﻿using Pixel.Automation.Core.Exceptions;
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

        protected IServiceResolver serviceProvider;
        private IFileSystem fileSystem;
        private IArgumentProcessor argumentProcessor;
        private IScriptEngine scriptEngine;
        private bool areDefaultServicesInitialized = false;
        private bool isPrimaryEntityManager = true;

        private Entity rootEntity;
        public Entity RootEntity
        {
            get => rootEntity;
            set
            {
                rootEntity = value;
                if(rootEntity != null)
                {
                    rootEntity.EntityManager = this;
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
            this.serviceProvider = serviceProvider;
            this.serviceProvider.RegisterDefault<IEntityManager>(this);
            this.serviceProvider.RegisterDefault<IServiceResolver>(serviceProvider);
        }

        /// <summary>
        /// Create EntityManager from an existing entity manager 
        /// </summary>
        /// <param name="entityManager"></param>
        public EntityManager(IEntityManager parentEntityManager)
        {
            this.serviceProvider = parentEntityManager.GetServiceOfType<IServiceResolver>() as IServiceResolver;
            this.RootEntity = parentEntityManager.RootEntity;
            this.isPrimaryEntityManager = false;
            this.SetCurrentFileSystem(parentEntityManager.GetCurrentFileSystem());
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
            return this.argumentProcessor;
        }

        public IScriptEngine GetScriptEngine()
        {
            return this.scriptEngine;
        }

        private void ConfigureServices()
        {
            IScriptEngineFactory scriptEngineFactory = this.GetServiceOfType<IScriptEngineFactory>();

            if (isPrimaryEntityManager)
            {
                scriptEngineFactory.WithSearchPaths(Environment.CurrentDirectory, fileSystem.ReferencesDirectory).
                WithAdditionalAssemblyReferences(fileSystem.GetAssemblyReferences()).
                WithAdditionalAssemblyReferences(this.arguments.GetType().Assembly);
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
            //TODO : We need to dispose components for non-primary Entity manager
            if (isDisposing && this.isPrimaryEntityManager)
            {
                foreach (var component in this.RootEntity.GetAllComponents())
                {
                    if (component is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                this.serviceProvider.Dispose();               
            }

            this.RootEntity = null;
            this.arguments = null;
            this.serviceProvider = null;
            this.fileSystem = null;

        }

        #endregion IDisposable
    }
}
