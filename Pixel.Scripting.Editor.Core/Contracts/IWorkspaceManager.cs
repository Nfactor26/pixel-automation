using Pixel.Scripting.Editor.Core.Models;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface IWorkspaceManager : IDisposable
    {
        string GetWorkingDirectory();

        /// <summary>
        /// Set current directory to override default working directory e.g. for a specific file that needs to be 
        /// saved somewhere else other than the default script directory
        /// </summary>
        /// <param name="currentyDirectory"></param>
        /// <returns></returns>
        void SetCurrentDirectory(string currentyDirectory);

        T GetService<T>();

        /// <summary>
        /// Add a new document to workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <param name="initialContent"></param>
        void AddDocument(string targetDocument, string initialContent);

        /// <summary>
        /// Remove document from workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <returns></returns>
        bool TryRemoveDocument(string targetDocument);

        /// <summary>
        /// Check whether the document is open in workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <returns></returns>
        bool IsDocumentOpen(string targetDocument);

        /// <summary>
        /// Open document in workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <returns></returns>
        bool TryOpenDocument(string targetDocument);

        /// <summary>
        /// Close document in workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <returns></returns>
        bool TryCloseDocument(string targetDocument);

        /// <summary>
        /// Check if a document already exists in workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <returns></returns>
        bool HasDocument(string targetDocument);

        /// <summary>
        /// Save document
        /// </summary>
        /// <param name="targetDocument"></param>
        void SaveDocument(string targetDocument);

        /// <summary>
        /// Get document contents
        /// </summary>
        /// <param name="targetDocument">Get the contents of document</param>
        /// <returns></returns>
        Task<string> GetBufferAsync(string targetDocument);

        Task UpdateBufferAsync(UpdateBufferRequest updateBufferRequest);

        Task ChangeBufferAsync(ChangeBufferRequest changeBufferRequest);

        void WithAssemblyReferences(string[] assemblyReferences);

        void WithAssemblyReferences(Assembly[] assemblyReferences);

    }

    public interface IScriptWorkspaceManager : IWorkspaceManager
    {

    }

    public interface ICodeWorkspaceManager : IWorkspaceManager
    {
        CompilationResult CompileProject(string outputAssemblyName);

    }
}
