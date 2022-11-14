using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
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
       
        /// <summary>
        /// Path of the assembly references file. This store references and imports for script editor, code editor and script engine for the project.
        /// </summary>
        string ReferencesFile { get; }

        /// <summary>
        /// Returns path relative to working directory of file system for a given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetRelativePath(string path); 

        /// <summary>
        /// Deserialize a given file to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetFile"></param>
        /// <returns></returns>
        T LoadFile<T>(string targetFile) where T : new();

        /// <summary>
        /// Deserialiize all the files in a directory to type T and return as a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="directory"></param>
        /// <returns></returns>
        IEnumerable<T> LoadFiles<T>(string directory) where T : new();

        /// <summary>
        /// Serialize the specified model and save in specified directory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="directory"></param>
        void SaveToFile<T>(T model, string directory) where T : new();

        /// <summary>
        /// Serialize the specified model and save it with specified file name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        void SaveToFile<T>(T model, string directory, string fileName) where T : new();

        /// <summary>
        /// Save the string contents in to specified file name
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        void CreateOrReplaceFile(string directory, string fileName, string content);

        /// <summary>
        /// Check if a path exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool Exists(string path);

        /// <summary>
        /// Read all the text from specified file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string ReadAllText(string path);
    }

    public interface IVersionedFileSystem : IFileSystem
    {
        /// <summary>
        /// Target version
        /// </summary>
        VersionInfo ActiveVersion { get; }

        /// <summary>
        /// Switch to a new target version
        /// </summary>
        /// <param name="version"></param>
        void SwitchToVersion(VersionInfo version);
    }

    public interface IProjectFileSystem : IVersionedFileSystem
    {
        /// <summary>
        /// Identifier of the project 
        /// </summary>
        string ProjectId { get; }

        /// <summary>
        /// Path to project file of the project
        /// </summary>
        string ProjectFile { get; }

        /// <summary>
        /// Path to process file for the project
        /// </summary>
        string ProcessFile { get; }
        
        /// <summary>
        /// Directory containing the test cases
        /// </summary>
        string TestCaseRepository { get; }

        /// <summary>
        /// Directory containing the test data files
        /// </summary>
        string TestDataRepository { get; }

        /// <summary>
        /// Initialize the project file system given automation project and version information
        /// </summary>       
        /// <param name="automationProject">Automation project</param>
        ///<param name="versionInfo">Target version of the project</param>
        void Initialize(AutomationProject automationProject, VersionInfo versionInfo);

        /// <summary>
        /// Get an instance of <see cref="TestCaseFiles"/> for a given TestCase
        /// </summary>       
        /// <returns></returns>
        TestCaseFiles GetTestCaseFiles(TestCase testCase);

        /// <summary>
        /// Get an instance of <see cref="TestFixtureFiles"/> for a given TestFixture
        /// </summary>       
        /// <returns></returns>
        TestFixtureFiles GetTestFixtureFiles(TestFixture fixture);

        /// <summary>
        /// Load TestDataSources available on local storage
        /// </summary>
        /// <returns></returns>
        IEnumerable<TestDataSource> GetTestDataSources();

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

        /// <summary>
        /// Initialize the prefab file system given prefab project and version information
        /// </summary>
        /// <param name="prefabProject">Prefab project</param>
        /// <param name="prefabVersion">Target version of the prefab</param>
        void Initialize(PrefabProject prefabProject, VersionInfo prefabVersion);            

        /// <summary>
        /// Get the data model assembly for the prefab
        /// </summary>
        /// <returns></returns>
        Assembly GetDataModelAssembly();
        
        /// <summary>
        /// Check if data model assembly has been updated
        /// </summary>
        /// <returns></returns>
        bool HasDataModelAssemblyChanged();

        /// <summary>
        /// Get the process entity for the prefab
        /// </summary>
        /// <returns></returns>
        Entity GetPrefabEntity();

        /// <summary>
        /// Save the specified entity as the prefab template
        /// </summary>
        /// <param name="templateRoot"></param>
        void CreateOrReplaceTemplate(Entity templateRoot);

        /// <summary>
        /// Get the available template if any for the prefab
        /// </summary>
        /// <returns></returns>
        Entity GetTemplate();

    }
   
}
