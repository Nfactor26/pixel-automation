using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pixel.Automation.RunTime
{
    /// <inheritdoc/>
    public class PrefabLoader : IPrefabLoader, IDisposable
    {
        private Entity prefabRoot;
        private Type dataModelType;
        private object dataModelInstance;
        private IPrefabFileSystem prefabFileSystem;
        private IEntityManager prefabManager;
        private bool isDisposed;

        public PrefabLoader()
        {            
        }


        public Entity LoadPrefab(string applicationId, string prefabId, PrefabVersion prefabVersion, IEntityManager entityManager)
        {
            Guard.Argument(applicationId).NotEmpty().NotNull();
            Guard.Argument(prefabId).NotEmpty().NotNull();
            Guard.Argument(entityManager).NotNull();
            Guard.Argument(prefabVersion).NotNull().Require(p => !string.IsNullOrEmpty(p.DataModelAssembly), p => { return "Assembly Name is not set on Prefab Version"; });

            if(isDisposed)
            {
                throw new InvalidOperationException($"{nameof(PrefabLoader)} is disposed.");
            }

            this.prefabManager = new EntityManager(entityManager);

            this.prefabFileSystem = this.prefabManager.GetServiceOfType<IPrefabFileSystem>();
            this.prefabFileSystem.Initialize(applicationId, prefabId, prefabVersion);
            this.prefabManager.SetCurrentFileSystem(this.prefabFileSystem);

            //Process entity manager should be able to resolve any assembly from prefab references folder such as prefab data model assembly 
            var scriptEngineFactory = entityManager.GetServiceOfType<IScriptEngineFactory>();
            scriptEngineFactory.WithAdditionalSearchPaths(this.prefabFileSystem.ReferencesDirectory);
          
            //Similalry, Script editor when working with prefab input and output mapping script should be able to resolve references from prefab references folder
            var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
            scriptEditorFactory.AddSearchPaths(this.prefabFileSystem.ReferencesDirectory);                    

            string prefabAssembly = Path.Combine(this.prefabFileSystem.ReferencesDirectory, prefabVersion.DataModelAssembly);
            if(!prefabFileSystem.Exists(prefabAssembly))
            {
                throw new FileNotFoundException($"Prefab data model assembly : {prefabAssembly} couldn't be located");
            }
            Assembly prefabDataModelAssembly = Assembly.LoadFrom(prefabAssembly);
                      
            this.dataModelType = prefabDataModelAssembly.GetTypes().FirstOrDefault(t => t.Name.Equals(Constants.PrefabDataModelName));
            if (this.dataModelType == null)
            {
                throw new NullReferenceException($"Failed to find type {Constants.PrefabDataModelName} in prefab data model assembly for prefab with applicationId : {applicationId}" +
                    $"and preafbId : {prefabId}");
            }
            this.dataModelInstance = Activator.CreateInstance(this.dataModelType);
    
            this.prefabManager.RootEntity = entityManager.RootEntity;

            this.prefabRoot = this.prefabFileSystem.GetPrefabEntity(prefabDataModelAssembly);
            this.prefabRoot.EntityManager = this.prefabManager;
            this.prefabManager.RestoreParentChildRelation(this.prefabRoot, false);
          
            //Setting argument will initialize all the required services such as script engine, argument processor , etc.
            this.prefabManager.Arguments = this.dataModelInstance;           

            return this.prefabRoot;
        }


        protected virtual void Dispose(bool isDisposing)
        {
            this.prefabManager?.Dispose();
            this.isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

    }
}
