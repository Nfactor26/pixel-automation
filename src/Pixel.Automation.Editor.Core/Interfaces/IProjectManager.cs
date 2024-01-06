using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Reference.Manager.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    /// <summary>
    /// Contract for managing a project
    /// </summary>
    public interface IProjectManager
    {
        /// <summary>
        /// Event handler that is triggered when the project is loaded
        /// </summary>
        event AsyncEventHandler<ProjectLoadedEventArgs> ProjectLoaded;

        /// <summary>
        /// Get the file systemm for the project
        /// </summary>
        /// <returns></returns>
        IFileSystem GetProjectFileSystem();

        /// <summary>
        /// Get the RefereManager for the project
        /// </summary>
        /// <returns></returns>
        IReferenceManager GetReferenceManager();

        /// <summary>
        /// Download a file associated with project given it's name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task DownloadFileByNameAsync(string fileName);

        /// <summary>
        /// Add a new data file or update an existing one for the loades version of project
        /// </summary>
        /// <param name="fileToDelete"></param>
        /// <returns></returns>
        Task AddOrUpdateDataFileAsync(string targetFile);

        /// <summary>
        /// Delete a data file belonging to the loaded version of project
        /// </summary>
        /// <param name="fileToDelete"></param>
        /// <returns></returns>
        Task DeleteDataFileAsync(string fileToDelete);

        /// <summary>
        /// Download the data model files (*.cs) belonging the the loaded version of project
        /// </summary>
        /// <returns></returns>
        Task DownloadDataModelFilesAsync();

        /// <summary>
        /// Deserialize the contents of a file to given type while ensuring that any references to data model assembly in file are replaced with current session's
        /// data model assembly. Use this to load a process file, prefab file or a test file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        T Load<T>(string fileName) where T : new();

        /// <summary>
        /// Reload the project
        /// </summary>
        /// <returns></returns>
        Task Reload();

        /// <summary>
        /// Save all project data
        /// </summary>
        /// <returns></returns>
        Task Save();
    }

    /// <summary>
    /// Contract for managing an automation project
    /// </summary>
    public interface IAutomationProjectManager: IProjectManager
    {
        /// <summary>
        /// Load specified version of the automation project
        /// </summary>
        /// <param name="activeProject"></param>
        /// <param name="versionToLoad"></param>
        /// <returns></returns>
        Task<Entity> Load(AutomationProject activeProject, VersionInfo versionToLoad);

        /// <summary>
        /// Update the state of script engine and script editors whenever version of prefab is changed
        /// </summary>       
        void OnPrefabVersionChanged(IEnumerable<PrefabReference> prefabs);
    }
    
    /// <summary>
    /// Contract for managing a prefab project
    /// </summary>
    public interface IPrefabProjectManager : IProjectManager
    {
        /// <summary>
        /// Load specified version of the prefab project
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <param name="versionInfo"></param>
        /// <returns></returns>
        Task<Entity> Load(PrefabProject prefabProject, VersionInfo versionInfo);
    }
}
