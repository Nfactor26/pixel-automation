using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Script.Editor.Script;
using System;

namespace Pixel.Scripting.Script.Editor
{
    public class ScriptEditorFactory : IScriptEditorFactory
    {      
        private readonly IEditorService editorService;
        private bool isInitialized = false;

        public ScriptEditorFactory(IEditorService editorService)
        {
            this.editorService = editorService;
        }

        public void Initialize(string workingDirectory, Type globalsType, string[] editorReferences)
        {
            this.isInitialized = true;
            this.editorService.Initialize(new WorkspaceOptions()
            { 
                WorkingDirectory = workingDirectory,
                EnableCodeActions = false,
                AssemblyReferences = editorReferences                
            }, globalsType);
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

        private void EnsureInitialized()
        {
            if (!isInitialized)
                throw new InvalidOperationException($"{nameof(ScriptEditorFactory)} is not yet initialized");
        }

        /// <summary>
        /// Create script editor that uses currentDirectory relative to workingDirectory to read and write files.
        /// </summary>
        /// <param name="currentDirectory"></param>
        /// <returns></returns>
        public IScriptEditorScreen CreateScriptEditor(string currentDirectory)
        {
            EnsureInitialized();
            this.editorService.SwitchToDirectory(currentDirectory);
            return new ScriptEditorScreenViewModel(this.editorService);
        }

        public IScriptWorkspaceManager GetWorkspaceManager()
        {
            IWorkspaceManager workspaceManager = this.editorService.GetWorkspaceManager();
            if (workspaceManager is IScriptWorkspaceManager scriptWorkspaceManager)
            {
                return scriptWorkspaceManager;
            }
            throw new Exception($"{nameof(IScriptWorkspaceManager)} is not available");
        }
    }
}
