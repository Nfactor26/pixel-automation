﻿using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Core
{
    public class PrefabFileSystem : VersionedFileSystem, IPrefabFileSystem
    {      
        private readonly string prefabsDirectory = "Prefabs";
        private string applicationId;
        private string prefabId;     
     
        public string PrefabDescriptionFile { get; private set; }

        public string PrefabFile { get; private set; }

        public string TemplateFile { get; private set; }
    
        public PrefabFileSystem(ISerializer serializer, ApplicationSettings applicationSettings) : base(serializer, applicationSettings)
        {

        }


        public void Initialize(string applicationId, string prefabId, VersionInfo version)
        {
            this.ActiveVersion = version;
            this.applicationId = applicationId;
            this.prefabId = prefabId;        

            this.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId, prefabsDirectory, prefabId, version.ToString());    
            this.PrefabDescriptionFile = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId, prefabsDirectory, prefabId, $"{prefabId}.dat");
            this.PrefabFile = Path.Combine(this.WorkingDirectory, Constants.PrefabEntityFileName);
            this.TemplateFile = Path.Combine(this.WorkingDirectory, Constants.PrefabTemplateFileName);
            
            base.Initialize();

            this.ReferenceManager = new AssemblyReferenceManager(this.applicationSettings, this.DataModelDirectory, this.ScriptsDirectory);

        }

        public void Initialize(string applicationId, string prefabId)
        {
            this.PrefabDescriptionFile = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId, prefabsDirectory, prefabId, $"{prefabId}.dat");
            PrefabProject prefabProject = this.serializer.Deserialize<PrefabProject>(this.PrefabDescriptionFile);
            var deployedVersions = prefabProject.DeployedVersions.OrderBy(a => a.Version);
            var latestVersion = deployedVersions.LastOrDefault();
            if (latestVersion == null)
            {
                throw new InvalidOperationException($"There is no deployed version for prefab : {prefabProject.PrefabName}");
            }
            Initialize(applicationId, prefabId, latestVersion);
        }

        public override void SwitchToVersion(VersionInfo version)
        {
            Initialize(applicationId, prefabId, version);
        }

        public Entity GetPrefabEntity()
        {
            if (!File.Exists(this.PrefabFile))
            {
                throw new FileNotFoundException($"{this.PrefabFile} not found");
            }
            Entity prefabRoot = this.serializer.Deserialize<Entity>(PrefabFile);
            return prefabRoot;
        }    

        public Entity GetPrefabEntity(Assembly withDataModelAssembly)
        {
            if (!File.Exists(this.PrefabFile))
            {
                throw new FileNotFoundException($"{this.PrefabFile} not found");
            }
            string fileContents = File.ReadAllText(this.PrefabFile);
            Regex regex = new Regex(@"(Pixel\.Automation\.Project\.DataModels)(\.\w*)(,\s)([\w|\d]*)");           
            fileContents = regex.Replace(fileContents, (m) =>
            {
                string result = m.Value.Replace($"{m.Groups[4].Value}", withDataModelAssembly.GetName().Name);         //replace assembly name
                result = result.Replace($"{m.Groups[1].Value}", withDataModelAssembly.GetTypes().First().Namespace);  //replace namesapce
                return result;
            });          
            Entity prefabRoot = this.serializer.DeserializeContent<Entity>(fileContents);
            return prefabRoot;
        }

        public Assembly GetDataModelAssembly()
        {
            var assemblyFiles = Directory.GetFiles(this.TempDirectory, "*.dll").Select(f => new FileInfo(Path.Combine(this.TempDirectory,f)));
            var targetFile = assemblyFiles.OrderBy(a => a.CreationTime).Last();
            return Assembly.LoadFrom(targetFile.FullName);          
        }

        public bool HasDataModelAssemblyChanged()
        {
            return true;
        }

        public void CreateOrReplaceTemplate(Entity templateRoot)
        {
            if(File.Exists(this.TemplateFile))
            {
                File.Delete(this.TemplateFile);
            }
            this.serializer.Serialize<Entity>(this.TemplateFile, templateRoot);
        }

        public Entity GetTemplate()
        {
            if(File.Exists(this.TemplateFile))
            {
                return this.serializer.Deserialize<Entity>(this.TemplateFile);
            }
            return default;
        }
    }
}
