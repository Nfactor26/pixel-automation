using Dawn;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Script.Editor.Script;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Scripting.Script.Editor
{
    public class ScriptEditorFactory : IScriptEditorFactory
    {
        private readonly ILogger logger = Log.ForContext<ScriptEditorFactory>();       
        private readonly IEditorService editorService;
        private bool isInitialized = false;
        private Dictionary<string, WeakReference<IInlineScriptEditor>> inlineEditors = new Dictionary<string, WeakReference<IInlineScriptEditor>>();
      
        public ScriptEditorFactory(IEditorService editorService)
        {
            Guard.Argument(editorService).NotNull();
            this.editorService = editorService;
            logger.Debug($"Create a new instance of {nameof(ScriptEditorFactory)}");
        }

        private void EnsureInitialized()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException($"{nameof(ScriptEditorFactory)} is not yet initialized");
            }
        }

        public void Initialize(string workingDirectory, IEnumerable<string> editorReferences, IEnumerable<string> imports)
        {
            Guard.Argument(workingDirectory).NotNull().NotEmpty();
         
            this.editorService.Initialize(new WorkspaceOptions()
            { 
                WorkingDirectory = workingDirectory,
                EnableCodeActions = true,
                AssemblyReferences = editorReferences,
                Imports = imports,
                WorkspaceType = WorkspaceType.Script
            });
            this.isInitialized = true;

            logger.Information("{0} is initialized now with working directory set to {1}.", nameof(ScriptEditorFactory), workingDirectory);
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

        public IInlineScriptEditor CreateInlineScriptEditor(EditorOptions editorOptions)
        {
            EnsureInitialized();
            return new InlineScriptEditorViewModel(this.editorService, editorOptions);
        }

        public IInlineScriptEditor CreateInlineScriptEditor(string cacheKey)
        {
            EnsureInitialized();
            IInlineScriptEditor editor;
            if (inlineEditors.ContainsKey(cacheKey))
            {
                if(inlineEditors[cacheKey].TryGetTarget(out editor))
                {
                    return editor;
                }
                inlineEditors.Remove(cacheKey);
            }

            editor = new InlineScriptEditorViewModel(this.editorService);
            inlineEditors.Add(cacheKey, new WeakReference<IInlineScriptEditor>(editor));
            logger.Debug("Created and cached inline script editor for {0}", cacheKey);
        
            return editor;          
        }

        public void RemoveInlineScriptEditor(string identifier)
        {
            if (inlineEditors.ContainsKey(identifier))
            {
                if (inlineEditors[identifier].TryGetTarget(out var editor))
                {
                    editor.Dispose();
                }
                inlineEditors.Remove(identifier);
                logger.Debug("Removed inline script editor for {0} from script editor factory cahce", identifier);
            }
        }

        public IScriptEditorScreen CreateScriptEditorScreen()
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

        public bool HasProject(string projectName)
        {
            return GetWorkspaceManager().HasProject(projectName);
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

        public void RemoveProject(string projectName)
        {
            this.editorService.TryRemoveProject(projectName);
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

        public void AddAssemblyReference(Assembly assembly)
        {
            Guard.Argument(assembly, nameof(assembly)).NotNull();
            var workSpaceManager = GetWorkspaceManager();
            workSpaceManager.WithAssemblyReferences(new[] { assembly });
            logger.Information($"Assembly : '{assembly.FullName}' reference was added to script editor worksapce");
        }

        public void RemoveAssemblyReference(Assembly assembly)
        {
            Guard.Argument(assembly, nameof(assembly)).NotNull();
            var workSpaceManager = GetWorkspaceManager();
            workSpaceManager.RemoveAssemblyReference(assembly);
            logger.Information($"Assembly : '{assembly.FullName}' reference was removed from script editor worksapce");
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            foreach(var inlineEditorRef in this.inlineEditors)
            {
                if(inlineEditorRef.Value.TryGetTarget(out var editor))
                {
                    editor.Dispose();
                }
            }
            this.inlineEditors.Clear();
            this.editorService.Dispose();
        }
    }
}
