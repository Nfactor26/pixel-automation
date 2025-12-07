using Dawn;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Script.Editor.Code;
using Pixel.Scripting.Script.Editor.MultiEditor;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Pixel.Scripting.Script.Editor
{
    public class CodeEditorFactory : ICodeEditorFactory
    {
        private readonly ILogger logger = Log.ForContext<CodeEditorFactory>();
        private readonly string Identifier = Guid.NewGuid().ToString();

        private readonly IEditorService editorService;
        private bool isInitialized = false;

        public CodeEditorFactory(IEditorService editorService)
        {
            Guard.Argument(editorService).NotNull();
            this.editorService = editorService;
            logger.Debug($"Create a new instance of {nameof(CodeEditorFactory)} with Id : {Identifier}");
        }

        public void Initialize(string workingDirectory, IEnumerable<string> editorReferences, IEnumerable<string> imports)
        {
            Guard.Argument(workingDirectory).NotNull().NotEmpty();
           
            if (this.isInitialized)
            {
                throw new InvalidOperationException($"{nameof(ScriptEditorFactory)} is already initialized.");
            }         
            this.editorService.Initialize(new WorkspaceOptions()
            {
                WorkingDirectory = workingDirectory,
                AssemblyReferences = editorReferences,
                WorkspaceType = WorkspaceType.Code
            });
            this.isInitialized = true;

            logger.Information($"{nameof(CodeEditorFactory)} is initialized now.");
        }

        public void SwitchWorkingDirectory(string workingDirectory)
        {
            this.editorService.SwitchToDirectory(workingDirectory);
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

        public IMultiEditor CreateMultiCodeEditorScreen()
        {
            EnsureInitialized();
            return new MultiEditorScreenViewModel(this.editorService);
        }

        public IMultiEditor CreateMultiCodeEditorControl()
        {
            EnsureInitialized();
            return new MultiEditorControlViewModel(this.editorService);
        }

        private void EnsureInitialized()
        {
            if (!isInitialized)
                throw new InvalidOperationException($"{nameof(CodeEditorFactory)} is not yet initialized");
        }

        private ICodeWorkspaceManager GetWorkspaceManager()
        {
            IWorkspaceManager workspaceManager = this.editorService.GetWorkspaceManager();
            if (workspaceManager is ICodeWorkspaceManager codeWorkspaceManager)
            {
                return codeWorkspaceManager;
            }
            throw new Exception($"{nameof(ICodeWorkspaceManager)} is not available");
        }

        public bool HasProject(string projectName)
        {
            return GetWorkspaceManager().HasProject(projectName);
        }

        public void AddProject(string projectName, string defaultNameSpace, string[] projectreferences)
        {
            var workSpaceManager = GetWorkspaceManager();
            if (!workSpaceManager.HasProject(projectName))
            {
                workSpaceManager.AddProject(projectName, defaultNameSpace, projectreferences);
                return;
            }
            logger.Information($"Project {projectName} already exists in workspace");
        }

        public void AddDocument(string documentName, string projectName, string documentContent)
        {
            var workSpaceManager = GetWorkspaceManager();           
            if (!workSpaceManager.HasDocument(documentName, projectName))
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
            if(workSpaceManager.HasProject(projectName))
            {
                workSpaceManager.RemoveProject(projectName);
                return;
            }
            logger.Information($"Project {projectName} doesn't exists in workspace. Can't remove project.");

        }

        public CompilationResult CompileProject(string projectName, string outputAssemblyName)
        {
            var workSpaceManager = GetWorkspaceManager();
            return workSpaceManager.CompileProject(projectName, outputAssemblyName);
        }

        public void AddAssemblyReference(Assembly assembly)
        {
            Guard.Argument(assembly, nameof(assembly)).NotNull();
            var workSpaceManager = GetWorkspaceManager();
            workSpaceManager.WithAssemblyReferences(new[] { assembly });
            logger.Debug("Assembly : '{0}' reference was added to code editor worksapce : {1}", assembly.FullName, this.Identifier);
        }

        public void AddAssemblyReferences(params string[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var reference = LoadReference(assembly);
                AddAssemblyReference(reference);
            }
        }

        public void RemoveAssemblyReference(Assembly assembly)
        {
            Guard.Argument(assembly, nameof(assembly)).NotNull();
            var workSpaceManager = GetWorkspaceManager();
            workSpaceManager.RemoveAssemblyReference(assembly);
            logger.Debug("Assembly : '{0}' reference was removed from code editor worksapce: {1}", assembly.FullName, this.Identifier);
        }

        public void RemoveAssemblyReferences(params string[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var reference = LoadReference(assembly);
                RemoveAssemblyReference(reference);
            }
        }

        private static Assembly LoadReference(string assembly)
        {
            Assembly reference = default;
            if (!Path.IsPathRooted(assembly))
            {
                string assemblyLocation = Path.Combine(Environment.CurrentDirectory, assembly);
                if (!File.Exists(assemblyLocation))
                {
                    throw new FileNotFoundException($"{assemblyLocation} was not found");
                }
                reference = Assembly.LoadFrom(assemblyLocation);
            }
            else
            {
                reference = Assembly.LoadFrom(assembly);
            }
            return reference;
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
