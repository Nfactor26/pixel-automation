using Pixel.Scripting.Editor.Core.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface IWorkspaceManager : IDisposable
    {
        /// <summary>
        /// Get the working directory
        /// </summary>
        /// <returns></returns>
        string GetWorkingDirectory();

        /// <summary>
        /// Change the working directory to a new location
        /// </summary>
        /// <param name="workingDirectory">Location of new working directory</param>
        void SwitchWorkingDirectory(string workingDirectory);

        /// <summary>
        /// Get service of specified type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// Update the source code for a document in workspace with new code
        /// </summary>
        /// <param name="updateBufferRequest"></param>
        /// <returns></returns>
        Task UpdateBufferAsync(UpdateBufferRequest updateBufferRequest);

        /// <summary>
        /// Replace a part of the source code for a document in workspace
        /// </summary>
        /// <param name="changeBufferRequest"></param>
        /// <returns></returns>
        Task ChangeBufferAsync(ChangeBufferRequest changeBufferRequest);

        /// <summary>
        /// Configure the workspace with additional assembly references that should be automatically added to any new project being added to workspace
        /// </summary>
        /// <param name="assemblyReferences"></param>
        void WithAssemblyReferences(IEnumerable<string> assemblyReferences);

        /// <summary>
        /// Configure the workspace with assembly references that should be automatically added to any new project being added to workspace
        /// </summary>
        /// <param name="assemblyReferences"></param>
        void WithAssemblyReferences(Assembly[] assemblyReferences);

        /// <summary>
        /// Remove an assembly reference from the workspace. Any existing project which already references the assembly will not be impacted
        /// </summary>
        /// <param name="assemblyReference">Assembly to remove</param>
        void RemoveAssemblyReference(Assembly assemblyReference);


    }

    public interface IScriptWorkspaceManager : IWorkspaceManager
    {
        /// <summary>
        /// Add a new project to the workspace
        /// </summary>
        /// <param name="projectName">Name of the project</param>
        /// <param name="projectReferences">References to other projects to be added</param>
        /// <param name="globalsType">Globals type for the project</param>
        void AddProject(string projectName, IEnumerable<string> projectReferences, Type globalsType);

        /// <summary>
        /// Add search paths to the ScriptMetadataResolver. This allows #r and #load references to be resolved from specified search paths.
        /// </summary>
        /// <param name="searchPaths"></param>
        void AddSearchPaths(params string[] searchPaths);

        /// <summary>
        /// Remove search paths from the ScriptMetadataResolver.
        /// </summary>
        /// <param name="searchPaths"></param>
        void RemoveSearchPaths(params string[] searchPaths);

        /// <summary>
        /// Add imports to the CompilationOptions. Adding imports help to avoid explicitly importing these references in scripts.
        /// This is helpful for inline script editors where we want some of the imports to be implicitly avialable.
        /// </summary>
        /// <param name="imports"></param>
        void AddImports(params string[] imports);

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
