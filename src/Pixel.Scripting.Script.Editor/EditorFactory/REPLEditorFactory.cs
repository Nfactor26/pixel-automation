using Pixel.Automation.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Script.Editor.REPL;
using System;
using System.Collections.Generic;
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

        public void Initialize(string workingDirectory, Type globalsType, IEnumerable<string> editorReferences)
        {
            this.isInitialized = true;
            this.editorService.Initialize(new WorkspaceOptions()
            {
                WorkingDirectory = workingDirectory,
                EnableCodeActions = false,              
                AssemblyReferences = editorReferences               
            });          
            scriptEngine.SetWorkingDirectory(Path.Combine(workingDirectory, "Temp"));           
        }

        public void SwitchWorkingDirectory(string workingDirectory)
        {
            this.editorService.SwitchToDirectory(workingDirectory);
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

        public void AddProject(string projectName, string[] projectreferences)
        {
            throw new NotImplementedException();
        }

        public void AddDocument(string documentName, string projectName, string documentContent)
        {
            throw new NotImplementedException();
        }

        public void RemoveDocument(string documentName, string projectName)
        {
            throw new NotImplementedException();
        }

        public void RemoveProject(string projectName)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string workingDirectory, IEnumerable<string> editorReferences, IEnumerable<string> imports)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            this.editorService.Dispose();
        }
    }
}
