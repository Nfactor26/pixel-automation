using Pixel.Automation.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Script.Editor.REPL;
using System;
using System.IO;

namespace Pixel.Scripting.Script.Editor
{
    public class REPLEditorFactory : IREPLEditorFactory
    {
        private readonly IScriptEngine scriptEngine;
        private readonly IEditorService editorService;       
        private bool isInitialized = false;

        public REPLEditorFactory(IScriptEngineFactory scriptEngineFactory, IEditorService editorService)
        {
            this.editorService = editorService;          
            this.scriptEngine = scriptEngineFactory.CreateInteractiveScriptEngine(); ;
        }

        public void Initialize(string workingDirectory, Type globalsType, string[] editorReferences)
        {
            this.isInitialized = true;
            this.editorService.Initialize(new EditorOptions()
            {
                WorkingDirectory = workingDirectory,
                EnableCodeActions = false,              
                EditorReferences = editorReferences               
            }, globalsType);          
            scriptEngine.SetWorkingDirectory(Path.Combine(workingDirectory, "Temp"));           
        }

        public IREPLScriptEditor CreateREPLEditor<T>(T globals)
        {
            EnsureInitialized();           
                   
            var interactiveEditor =  new REPLEditorScreenViewModel(this.editorService, this.scriptEngine);
            interactiveEditor.SetGlobals(globals);
            return interactiveEditor;
        }

        private void EnsureInitialized()
        {
            if (!isInitialized)
                throw new InvalidOperationException($"{nameof(REPLEditorFactory)} is not yet initialized");
        }
    }
}
