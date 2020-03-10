using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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


        public Entity LoadPrefab(string applicationId, string prefabId, Version prefabVersion, EntityManager primaryEntityManager)
        {

            IServiceResolver serviceResolver = primaryEntityManager.GetServiceOfType<IServiceResolver>();
            this.prefabManager = new EntityManager(primaryEntityManager, null);       

            this.prefabFileSystem = this.prefabManager.GetServiceOfType<IPrefabFileSystem>();
            if (prefabVersion != null)
            {
                this.prefabFileSystem.Initialize(applicationId, prefabId, prefabVersion);
            }
            else
            {
                this.prefabFileSystem.Initialize(applicationId, prefabId);
            }
            this.prefabManager.RegisterDefault<IFileSystem>(this.prefabFileSystem);
            this.prefabManager.SetCurrentFileSystem(this.prefabFileSystem);
            this.prefabManager.Environment = primaryEntityManager.Environment;
            this.prefabManager.WorkingDirectory = this.prefabFileSystem.WorkingDirectory;


            IWorkspaceManagerFactory workspaceManagerFactory = this.prefabManager.GetServiceOfType<IWorkspaceManagerFactory>();
            IWorkspaceManager workspaceManager = workspaceManagerFactory.CreateCodeWorkspaceManager(this.prefabFileSystem.DataModelDirectory);
            workspaceManager.WithAssemblyReferences(this.prefabFileSystem.GetAssemblyReferences());
            workspaceManager.WithAssemblyReferences(new[] { primaryEntityManager.Arguments.GetType().Assembly });
            foreach(var file in Directory.GetFiles(this.prefabFileSystem.DataModelDirectory,"*.cs"))
            {
                workspaceManager.AddDocument(Path.GetFileName(file), File.ReadAllText(file));
            }

            string assemblyName = Guid.NewGuid().ToString();
            var compilationResult = (workspaceManager as ICodeWorkspaceManager).CompileProject(assemblyName);
            compilationResult.SaveAssemblyToDisk(Path.Combine(this.prefabFileSystem.DataModelDirectory, "Temp"));
            Assembly prefabDataModelAssembly = Assembly.LoadFrom(Path.Combine(this.prefabFileSystem.DataModelDirectory, "Temp", $"{assemblyName}.dll"));

            this.dataModelType = prefabDataModelAssembly.GetTypes().FirstOrDefault();
            if (this.dataModelType == null)
            {
                throw new NullReferenceException($"Failed to find any types defined in prefab data model assembly with applicationId : {applicationId}" +
                    $"and preafbId : {prefabId}");
            }
            this.dataModelInstance = Activator.CreateInstance(this.dataModelType);
    
            this.prefabManager.RootEntity = primaryEntityManager.RootEntity;         

            this.prefabRoot = this.prefabFileSystem.GetPrefabEntity();
            this.prefabRoot.EntityManager = this.prefabManager;
            this.prefabManager.RestoreParentChildRelation(this.prefabRoot, false);
          
            this.prefabManager.Arguments = this.dataModelInstance;
        
            return this.prefabRoot;
        }
    }
}
