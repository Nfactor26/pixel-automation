using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Implementation of <see cref="IPrefabFileSystem"/>
    /// </summary>
    public class PrefabFileSystem : VersionedFileSystem, IPrefabFileSystem
    {
        private string applicationId;
        private string prefabId;

        /// <InheritDoc/>      
        public string PrefabDescriptionFile { get; private set; }

        /// <InheritDoc/>      
        public string PrefabFile { get; private set; }

        /// <InheritDoc/>      
        public string TemplateFile { get; private set; }
    
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="applicationSettings"></param>
        public PrefabFileSystem(ISerializer serializer, ApplicationSettings applicationSettings) : base(serializer, applicationSettings)
        {

        }

        /// <InheritDoc/>      
        public void Initialize(PrefabProject prefabProject, VersionInfo versionInfo)
        {
            Guard.Argument(prefabProject).NotNull();
            this.Initialize(prefabProject.ApplicationId, prefabProject.PrefabId, versionInfo);
        }

        /// <InheritDoc/>      
        public override void SwitchToVersion(VersionInfo version)
        {
            Initialize(this.applicationId, this.prefabId, version);
        }

        /// <InheritDoc/>      
        void Initialize(string applicationId, string prefabId, VersionInfo versionInfo)
        {          
            this.applicationId = Guard.Argument(applicationId).NotNull().NotEmpty();
            this.prefabId = Guard.Argument(prefabId).NotNull().NotEmpty();
            this.ActiveVersion = Guard.Argument(versionInfo).NotNull();

            this.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId, Constants.PrefabsDirectory, prefabId, versionInfo.ToString());
            this.PrefabDescriptionFile = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId, Constants.PrefabsDirectory, prefabId, $"{prefabId}.atm");                    
            this.PrefabFile = Path.Combine(this.WorkingDirectory, Constants.PrefabProcessFileName);
            this.TemplateFile = Path.Combine(this.WorkingDirectory, Constants.PrefabTemplateFileName);

            base.Initialize();

            this.ReferenceManager = new AssemblyReferenceManager(this.applicationSettings, this.DataModelDirectory, this.ScriptsDirectory);

        }

        /// <InheritDoc/>      
        public Entity GetPrefabEntity()
        {
            if (!File.Exists(this.PrefabFile))
            {
                throw new FileNotFoundException($"{this.PrefabFile} not found");
            }
            Entity prefabRoot = this.serializer.Deserialize<Entity>(PrefabFile);
            return prefabRoot;
        }

        /// <InheritDoc/>      
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

        /// <InheritDoc/>      
        public Assembly GetDataModelAssembly()
        {
            var assemblyFiles = Directory.GetFiles(this.TempDirectory, "*.dll").Select(f => new FileInfo(Path.Combine(this.TempDirectory,f)));
            var targetFile = assemblyFiles.OrderBy(a => a.CreationTime).Last();
            return Assembly.LoadFrom(targetFile.FullName);          
        }

        /// <InheritDoc/>      
        public bool HasDataModelAssemblyChanged()
        {
            return true;
        }

        /// <InheritDoc/>      
        public void CreateOrReplaceTemplate(Entity templateRoot)
        {
            if(File.Exists(this.TemplateFile))
            {
                File.Delete(this.TemplateFile);
            }
            this.serializer.Serialize<Entity>(this.TemplateFile, templateRoot);
        }

        /// <InheritDoc/>      
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
