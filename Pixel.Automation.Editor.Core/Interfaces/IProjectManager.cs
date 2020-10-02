using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IProjectManager
    {
        IProjectManager WithEntityManager(EntityManager entityManager);

        IFileSystem GetProjectFileSystem();

        IScriptEditorScreen CreateScriptEditor(string projectId, string[] projectReferences, Type globalsType, string scriptFile);

        /// <summary>
        /// Save all project data
        /// </summary>
        /// <returns></returns>
        Task Save();

        /// <summary>
        /// Deserialize the contents of a file to given type while ensuring that any references to data model assembly in file are replaced with current session's
        /// data model assembly. Use this to load a process file, prefab file or a test file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        T Load<T>(string fileName) where T : new();
    }
}
