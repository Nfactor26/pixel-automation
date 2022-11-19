using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Reference.Manager.Contracts;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IProjectManager
    {      
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

    public interface IAutomationProjectManager: IProjectManager
    {
        Task<Entity> Load(AutomationProject activeProject, VersionInfo versionToLoad);       
    }
    

    public interface IPrefabProjectManager : IProjectManager
    {
        Task<Entity> Load(PrefabProject prefabProject, VersionInfo versionInfo);
    }
}
