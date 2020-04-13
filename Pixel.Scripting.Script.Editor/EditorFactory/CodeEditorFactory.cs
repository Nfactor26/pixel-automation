using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Script.Editor.Code;
using Pixel.Scripting.Script.Editor.MultiEditor;
using System;

namespace Pixel.Scripting.Script.Editor
{
    public class CodeEditorFactory : ICodeEditorFactory
    {
        private readonly IEditorService editorService;
        private bool isInitialized = false;

        public CodeEditorFactory(IEditorService editorService)
        {
            this.editorService = editorService;
        }

        public void Initialize(string workingDirectory, string[] editorReferences)
        {
            this.isInitialized = true;
            this.editorService.Initialize(new EditorOptions()
            {
                WorkingDirectory = workingDirectory,
                EditorReferences = editorReferences
            }, null);
        }

        public ICodeEditorScreen CreateCodeEditor()
        {
            EnsureInitialized();
            return new CodeEditorScreenViewModel(this.editorService);
        }

        public ICodeEditorControl CreateCodeEditorControl()
        {
            EnsureInitialized();
            return new CodeEditorControlViewModel(this.editorService);
        }

        public IMultiEditor CreateMultiCodeEditor()
        {
            EnsureInitialized();
            return new MultiEditorViewModel(this.editorService);
        }

        private void EnsureInitialized()
        {
            if (!isInitialized)
                throw new InvalidOperationException($"{nameof(CodeEditorFactory)} is not yet initialized");
        }

        public ICodeWorkspaceManager GetWorkspaceManager()
        {
            IWorkspaceManager workspaceManager = this.editorService.GetWorkspaceManager();
            if (workspaceManager is ICodeWorkspaceManager codeWorkspaceManager)
            {
                return codeWorkspaceManager;
            }
            throw new Exception($"{nameof(ICodeWorkspaceManager)} is not available");
        }

       
    }
}
