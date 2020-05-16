using Pixel.Automation.Core.Models;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Core.Interfaces
{

    public interface IFileSystem
    {      
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

        /// <summary>
        /// All assemblies in this folder will be added as references to editors and script engine
        /// </summary>
        string ReferencesDirectory { get;  }

        string[] GetAssemblyReferences();      
       
        IEnumerable<T> LoadFiles<T>(string directory) where T : new();

        void SaveToFile<T>(T model, string directory) where T : new();

        void SaveToFile<T>(T model, string directory, string fileName) where T : new();

        void CreateOrReplaceFile(string directory, string fileName, string content);

        bool Exists(string path);

        string ReadAllText(string path);
    }

    public interface IVersionedFileSystem : IFileSystem
    {
        VersionInfo ActiveVersion { get; }

        void SwitchToVersion(VersionInfo version);
    }


    public interface IProjectFileSystem : IVersionedFileSystem
    {
        string ProjectFile { get; }

        string ProcessFile { get; }

        string TestCaseRepository { get; }

        string TestDataRepository { get; }

        /// <summary>
        /// Initialize the file system by providing workingdirectory and projectname
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="projectId"></param>
        void Initialize(string projectId, VersionInfo versionInfo);

        ITestCaseFileSystem CreateTestCaseFileSystemFor(string testCaseId);
      
    }

    public interface IPrefabFileSystem : IVersionedFileSystem
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

        void Initialize(string applicationId, string prefabId, VersionInfo prefabVersion);

        /// <summary>
        /// Initialize prefab file system for deployed version
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        void Initialize(string applicationId, string prefabId);

        Assembly GetDataModelAssembly();
        
        bool HasDataModelAssemblyChanged();

        Entity GetPrefabEntity();

        /// <summary>
        /// Load Prefab Entity and update all data model reference to use this new data model assembly.
        /// This is required because every time prefab data model is compiled a new assembly is generated.
        /// </summary>
        /// <param name="withDataModelAssembly"></param>
        /// <returns></returns>
        Entity GetPrefabEntity(Assembly withDataModelAssembly);

        void CreateOrReplaceTemplate(Entity templateRoot);

        Entity GetTemplate();

    }

    public interface ITestCaseFileSystem : IFileSystem
    {
        /// <summary>
        /// Path of test case directory which contains test case file, test process file and test scripts
        /// </summary>
        string TestDirectory { get; }
        
        /// <summary>
        /// Path of Test Case file
        /// </summary>
        string TestCaseFile { get; }

        /// <summary>
        /// Path of Test automation process file
        /// </summary>
        string TestProcessFile { get; }


        void Initialize(string projectWorkingDirectory, string testCaseId);
    }
}
