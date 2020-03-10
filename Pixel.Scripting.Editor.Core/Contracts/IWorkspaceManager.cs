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

        void AddDocument(string documentName, string initialContent);

        bool TryRemoveDocument(string documentName);

        bool IsDocumentOpen(string documentName);

        bool TryOpenDocument(string documentName);

        bool TryCloseDocument(string documentName);

        bool HasDocument(string documentName);

        void SaveDocument(string documentName);

        Task<string> GetBufferAsync(string documentName);

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
