using Dawn;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Script.Editor.Script;
using Serilog;
using System;

namespace Pixel.Scripting.Script.Editor
{
    public class ScriptEditorFactory : IScriptEditorFactory
    {
        private readonly ILogger logger = Log.ForContext<ScriptEditorFactory>();
        private readonly IEditorService editorService;
        private bool isInitialized = false;
      
        public ScriptEditorFactory(IEditorService editorService)
        {
            this.editorService = editorService;
        }

        private void EnsureInitialized()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException($"{nameof(ScriptEditorFactory)} is not yet initialized");
            }
        }


        public void Initialize(string workingDirectory, string[] editorReferences)
        {
            this.isInitialized = true;
            this.editorService.Initialize(new WorkspaceOptions()
            { 
                WorkingDirectory = workingDirectory,
                EnableCodeActions = true,
                AssemblyReferences = editorReferences,
                WorkspaceType = WorkspaceType.Script
            });
            logger.Information($"{nameof(ScriptEditorFactory)} is initialized now.");
        }

        public void SwitchWorkingDirectory(string workingDirectory)
        {
            this.editorService.SwitchToDirectory(workingDirectory);
        }

        public IInlineScriptEditor CreateInlineScriptEditor()
        {
            EnsureInitialized();
            return new InlineScriptEditorViewModel(this.editorService);
        }

        public IScriptEditorScreen CreateScriptEditor()
        {
            EnsureInitialized();
            return new ScriptEditorScreenViewModel(this.editorService);
        }      
      
        private IScriptWorkspaceManager GetWorkspaceManager()
        {
            IWorkspaceManager workspaceManager = this.editorService.GetWorkspaceManager();
            if (workspaceManager is IScriptWorkspaceManager scriptWorkspaceManager)
            {
                return scriptWorkspaceManager;
            }
            throw new Exception($"{nameof(IScriptWorkspaceManager)} is not available");
        }

        public void AddProject(string projectName, string[] projectreferences, Type globalsType)
        {
            var workSpaceManager = GetWorkspaceManager();
            if (!workSpaceManager.HasProject(projectName))
            {
                workSpaceManager.AddProject(projectName, projectreferences, globalsType);
                return;
            }
            logger.Information($"Project {projectName} already exists in workspace");
        }

        public void AddDocument(string documentName, string projectName, string documentContent)
        {
            var workSpaceManager = GetWorkspaceManager();

            if(!workSpaceManager.HasDocument(documentName, projectName))
            {
                workSpaceManager.AddDocument(documentName, projectName, documentContent);
                return;
            }
            logger.Information($"Document {documentName} already exists in project {projectName}");
        }

        public void RemoveDocument(string documentName, string projectName)
        {
            var workSpaceManager = GetWorkspaceManager();
            if (workSpaceManager.HasDocument(documentName, projectName))
            {
                workSpaceManager.TryRemoveDocument(documentName, projectName);
                return;
            }
            logger.Information($"Document {documentName} doesn't exists in project {projectName}. Can't remove document.");            
        }

        public void RemoveProject(string projectName)
        {
            var workSpaceManager = GetWorkspaceManager();
            if (workSpaceManager.HasProject(projectName))
            {
                workSpaceManager.RemoveProject(projectName);
                return;
            }
            logger.Information($"Project {projectName} doesn't exists in workspace. Can't remove project.");
        }

        public void AddSearchPaths(params string[] searchPaths)
        {
            Guard.Argument(searchPaths).NotEmpty();
            var workSpaceManager = GetWorkspaceManager();
            workSpaceManager.AddSearchPaths(searchPaths);
        }

        public void RemoveSearchPaths(params string[] searchPaths)
        {
            Guard.Argument(searchPaths).NotEmpty();
            var workSpaceManager = GetWorkspaceManager();
            workSpaceManager.RemoveSearchPaths(searchPaths);
        }
    }
}
