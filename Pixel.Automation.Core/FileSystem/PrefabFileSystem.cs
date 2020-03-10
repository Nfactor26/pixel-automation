using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pixel.Automation.Core
{
    public class PrefabFileSystem : FileSystem, IPrefabFileSystem
    {
        private readonly string applicationsDirectory = "ApplicationsRepository";
        private readonly string prefabsDirectory = "Prefabs";
        private string applicationId;
        private string prefabId;
     
        public string PrefabDescriptionFile { get; private set; }

        public string PrefabFile { get; private set; }

        public string TemplateFile { get; private set; }

     
        public PrefabFileSystem(ISerializer serializer) : base(serializer)
        {

        }


        public void Initialize(string applicationId, string prefabId, Version version)
        {
            this.ActiveVersion = version;
            this.applicationId = applicationId;
            this.prefabId = prefabId;        

            this.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, applicationsDirectory, applicationId, prefabsDirectory, prefabId, version.ToString());    
            this.PrefabDescriptionFile = Path.Combine(Environment.CurrentDirectory, applicationsDirectory, applicationId, prefabsDirectory, prefabId, "PrefabDescription.dat");
            this.PrefabFile = Path.Combine(this.WorkingDirectory, "Prefab.dat");
            this.TemplateFile = Path.Combine(Environment.CurrentDirectory, applicationsDirectory, applicationId, prefabsDirectory, prefabId, "Template.dat");

            base.Initialize();
        }

        public void Initialize(string applicationId, string prefabId)
        {
            this.PrefabDescriptionFile = Path.Combine(Environment.CurrentDirectory, applicationsDirectory, applicationId, prefabsDirectory, prefabId, "PrefabDescription.dat");
            PrefabDescription prefabDescription = this.serializer.Deserialize<PrefabDescription>(this.PrefabDescriptionFile);
            if(prefabDescription.DeployedVersion == null)
            {
                throw new InvalidOperationException($"There is no deployed version for prefab : {prefabDescription.PrefabName}");
            }
            Initialize(applicationId, prefabId, prefabDescription.DeployedVersion);
        }

        public override void SwitchToVersion(Version version)
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
