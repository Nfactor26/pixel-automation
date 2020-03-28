using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Core.Interfaces
{

    public interface IFileSystem
    {
        Version ActiveVersion { get; }

        /// <summary>
        /// Working directory for a given project or prefab
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        /// Directory where the scripts are stored
        /// </summary>
        string ScriptsDirectory { get; }
        
        /// <summary>
        /// Directory where temporary files are stored
        /// </summary>
        string TempDirectory { get; }
        
        /// <summary>
        /// Directory where datamodel.cs and any related files are stored
        /// </summary>
        string DataModelDirectory { get; }

        string[] GetKnownDirectories();

        string[] GetAssemblyReferences();

        void SwitchToVersion(Version version);

        IEnumerable<T> LoadFiles<T>(string directory) where T : new();

        void SaveToFile<T>(T model, string directory) where T : new();

        void SaveToFile<T>(T model, string directory, string fileName) where T : new();

        void CreateOrReplaceFile(string directory, string fileName, string content);
    }

    public interface IProjectFileSystem : IFileSystem
    {              
        string ProjectFile { get; }

        string ProcessFile { get; }

        string TestCaseDirectory { get; }

        string TestDataRepoDirectory { get; }

        /// <summary>
        /// Initialize the file system by providing workingdirectory and projectname
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="projectId"></param>
        void Initialize(string projectId, Version version);

        /// <summary>
        /// Initialize the file system for specified projectId.
        /// Deployed version is used for initialization.
        /// </summary>
        /// <param name="projectId"></param>
        void Initialize(string projectId);
    }

    public interface IPrefabFileSystem : IFileSystem
    {   
        /// <summary>
        /// Prefab description file 
        /// </summary>
        string PrefabDescriptionFile { get; }

        /// <summary>
        /// Prefab data file that contains serialized entity
        /// </summary>
        string PrefabFile { get; }
        
        /// <summary>
        /// While editing a prefab , Prefab is embedded in to this template.
        /// Single template exists irrespective of version of prefab
        /// </summary>
        string TemplateFile { get; }     

        void Initialize(string applicationId, string prefabId, Version prefabVersion);

        /// <summary>
        /// Initialize prefab file system for deployed version
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        void Initialize(string applicationId, string prefabId);

        Assembly GetDataModelAssembly();
        
        bool HasDataModelAssemblyChanged();

        Entity GetPrefabEntity();
  
        void CreateOrReplaceTemplate(Entity templateRoot);

        Entity GetTemplate();

    }
}
