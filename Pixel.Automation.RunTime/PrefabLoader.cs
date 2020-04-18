﻿using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Pixel.Automation.Core.Models;
using Dawn;

namespace Pixel.Automation.RunTime
{
    public class PrefabLoader : IPrefabLoader
    {
        private Entity prefabRoot;
        private Type dataModelType;
        private object dataModelInstance;
        private IPrefabFileSystem prefabFileSystem;
        private EntityManager prefabManager;      

        public PrefabLoader()
        {            
        }


        public Entity LoadPrefab(string applicationId, string prefabId, PrefabVersion prefabVersion, EntityManager entityManager)
        {
            Guard.Argument(applicationId).NotEmpty().NotNull();
            Guard.Argument(prefabId).NotEmpty().NotNull();
            Guard.Argument(entityManager).NotNull();
            Guard.Argument(prefabVersion).NotNull().Require(p => !string.IsNullOrEmpty(p.PrefabAssembly), p => { return "Assembly Name is not set on Prefab Version"; });   
         
         
            this.prefabManager = new EntityManager(entityManager, null);   

            this.prefabFileSystem = this.prefabManager.GetServiceOfType<IPrefabFileSystem>();
            this.prefabFileSystem.Initialize(applicationId, prefabId, prefabVersion);
            
            this.prefabManager.RegisterDefault<IFileSystem>(this.prefabFileSystem);
            this.prefabManager.SetCurrentFileSystem(this.prefabFileSystem);         
            this.prefabManager.WorkingDirectory = this.prefabFileSystem.WorkingDirectory;

            string prefabAssembly = Path.Combine(Environment.CurrentDirectory, prefabVersion.PrefabAssembly);
            if(!File.Exists(prefabAssembly))
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
    }
}
