using Pixel.Scripting.Editor.Core.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface IWorkspaceManager : IDisposable
    {
        string GetWorkingDirectory();

        /// <summary>
        /// Change the working directory to a new location
        /// </summary>
        /// <param name="workingDirectory">Location of new working directory</param>
        void SwitchWorkingDirectory(string workingDirectory);

        T GetService<T>();

        /// <summary>
        /// Check if specified project is available in workspace
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        bool HasProject(string projectName);      

        /// <summary>
        /// Remove specified project from workspace
        /// </summary>
        /// <param name="projectName">Name of the project to remove</param>
        void RemoveProject(string projectName);

        /// <summary>
        /// Get the names of all projects available in workspace
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllProjects();

        /// <summary>
        /// Get the defualt namespace for a given project
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        string GetDefaultNameSpace(string projectName);

        /// <summary>
        /// Add a new document to workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <param name="initialContent"></param>
        /// <param name="addToProject">Name of the project to which document should be added</param>
        void AddDocument(string targetDocument, string addToProject, string initialContent);

        /// <summary>
        /// Remove document from workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <param name="removeFromProject">Name of the project from which document should be removed</param>
        /// <returns></returns>
        bool TryRemoveDocument(string targetDocument, string removeFromProject);

        /// <summary>
        /// Check whether the document is open in workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <param name="ownerProject">Name of the project to which document belongs</param>
        /// <returns></returns>
        bool IsDocumentOpen(string targetDocument, string ownerProject);

        /// <summary>
        /// Open document in workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <param name="ownerProject">Name of the project to which document belongs</param>
        /// <returns></returns>
        bool TryOpenDocument(string targetDocument, string ownerProject);

        /// <summary>
        /// Close document in workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <param name="ownerProject">Name of the project to which document belongs</param>
        /// <returns></returns>
        bool TryCloseDocument(string targetDocument, string ownerProject);

        /// <summary>
        /// Check if a document already exists in workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <param name="ownerProject">Name of the project to which document belongs</param>
        /// <returns></returns>
        bool HasDocument(string targetDocument, string ownerProject);

        /// <summary>
        /// Save document
        /// </summary>
        /// <param name="targetDocument"></param>
        /// <param name="ownerProject">Name of the project to which document belongs</param>
        void SaveDocument(string targetDocument, string ownerProject);

        /// <summary>
        /// Get document contents
        /// </summary>
        /// <param name="targetDocument">Get the contents of document</param>
        /// <param name="ownerProject">Name of the project to which document belongs</param>
        /// <returns></returns>
        Task<string> GetBufferAsync(string targetDocument, string ownerProject);

        Task UpdateBufferAsync(UpdateBufferRequest updateBufferRequest);

        Task ChangeBufferAsync(ChangeBufferRequest changeBufferRequest);

        void WithAssemblyReferences(string[] assemblyReferences);

        void WithAssemblyReferences(Assembly[] assemblyReferences);


    }

    public interface IScriptWorkspaceManager : IWorkspaceManager
    {
        void AddProject(string projectName, IEnumerable<string> projectReferences, Type globalsType);

        void AddSearchPaths(params string[] searchPaths);

        void RemoveSearchPaths(params string[] searchPaths);
    }

    public interface ICodeWorkspaceManager : IWorkspaceManager
    {        
        /// <summary>
        /// Add a new project to the workspace
        /// </summary>
        /// <param name="projectName">Name of project</param>
        /// <param name="defaultNameSpace">Default namespace for project</param>
        /// <param name="projectReferences">Other projects from workspace to be referenced</param>
        void AddProject(string projectName, string defaultNameSpace, IEnumerable<string> projectReferences);
    

        /// <summary>
        /// Compile the project
        /// </summary>
        /// <param name="projectName">Name of project</param>
        /// <param name="outputAssemblyName">Name of output assembly</param>
        /// <returns>CompilationResult</returns>
        CompilationResult CompileProject(string projectName, string outputAssemblyName);

    }
}
