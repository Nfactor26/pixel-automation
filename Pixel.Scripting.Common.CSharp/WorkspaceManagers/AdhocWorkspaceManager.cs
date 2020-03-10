using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Editor.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Scripting.Common.CSharp.WorkspaceManagers
{
    /// <summary>
    /// AdhocWorkspaceManager wraps a AdhocWorkspace and should be used for script execution
    /// </summary>
    public abstract class AdhocWorkspaceManager : IWorkspaceManager
    {
        private string currentDirectory;
        private string workingDirectory;
        protected AdhocWorkspace workspace = default;

        protected readonly string defaultProjectName = "Scripts";
        protected readonly string tempFolder = "Temp";

        protected List<Assembly> additionalReferences;
       
        public AdhocWorkspaceManager(string workingDirectory)
        {
            Guard.Argument(workingDirectory).NotNull().NotEmpty();

            if (!Directory.Exists(Path.Combine(workingDirectory, "Temp")))
            {
                Directory.CreateDirectory(Path.Combine(workingDirectory, "Temp"));
            }
            this.workingDirectory = workingDirectory;

            InitializeCompositionHost();
       
            workspace = new AdhocWorkspace(HostServices);
            workspace.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create()));
            this.additionalReferences = new List<Assembly>();
        }

        protected abstract ProjectId CreateNewProject();

        protected virtual CSharpCompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: ProjectReferences.DesktopDefault.Imports);
            return compilationOptions;
        }
      
        public Workspace GetWorkspace()
        {
            return this.workspace;
        }

        public bool TryGetProject(string documentName, out Project project)
        {        
            foreach (var proj in this.workspace.CurrentSolution.Projects)
            {
                foreach (var document in proj.Documents)
                {
                    if (document.Name.Equals(documentName))
                    {
                        project = proj;
                        return true;
                    }
                }
            }
            project = default;
            return false;
        }

        public bool TryGetDocument(string documentName, out Document document)
        {
            Guard.Argument(documentName).NotNull().NotEmpty();
         
            foreach (var project in this.workspace.CurrentSolution.Projects)
            {
                foreach (var doc in project.Documents)
                {
                    if (doc.Name.Equals(documentName))
                    {
                        document = doc;
                        return true;
                    }
                }
            }
            document = default;
            return false;
        }

        public Document GetDocumentById(DocumentId documentId)
        {
            foreach (var project in this.workspace.CurrentSolution.Projects)
            {
                foreach (var doc in project.Documents)
                {
                    if (doc.Id.Equals(documentId))
                    {
                        return doc;
                    }
                }
            }
            throw new ArgumentException($"Document with id {documentId} doesn't exist in workspace");
        }

        #region IWorkspaceManager

        public string GetWorkingDirectory()
        {
            return this.currentDirectory ?? this.workingDirectory;
        }

        public void SetCurrentDirectory(string currentDirectory)
        {
            this.currentDirectory = currentDirectory;
        }

        //public override IObservable<T> GetDiagnosticsUpdateObservable<T>()
        //{
        //    IDiagnosticService diagnosticService = GetService<IDiagnosticService>();
        //    var observable = Observable.FromEventPattern<T>(diagnosticService, nameof(diagnosticService.DiagnosticsUpdated));
        //    return observable;
        //}

        public abstract void AddDocument(string documentName, string initialContent);
        
        public bool TryRemoveDocument(string documentName)
        {
            if(TryGetDocument(documentName,out Document document))
            {
                var project = document.Project;
                project = project.RemoveDocument(document.Id);
                return workspace.TryApplyChanges(project.Solution);
            }
            return false;
        }

        public bool IsDocumentOpen(string documentName)
        {
            if (TryGetDocument(documentName, out Document document))
            {
                return workspace.IsDocumentOpen(document.Id);               
            }
            return false;
        }

        public bool TryOpenDocument(string documentName)
        {
            if (TryGetDocument(documentName, out Document document))
            {
                if(!workspace.IsDocumentOpen(document.Id))
                {
                    workspace.OpenDocument(document.Id);
                    return true;
                }               
            }
            return false;        
        }

        /// <summary>
        /// Close document and remove owner project from workspace
        /// </summary>
        /// <param name="documentName"></param>
        /// <returns></returns>
        public virtual bool TryCloseDocument(string documentName)
        {
            if (TryGetDocument(documentName, out Document document))
            {
                if (workspace.IsDocumentOpen(document.Id))
                {
                    workspace.CloseDocument(document.Id);
                    var updatedSolution = workspace.CurrentSolution.RemoveProject(document.Project.Id);
                    workspace.TryApplyChanges(updatedSolution);
                    return true;
                }               
            }
            return false;           
        }

        public bool HasDocument(string documentName)
        {
            return TryGetDocument(documentName, out Document document);
        }
       
        public void SaveDocument(string documentName)
        {
            if (TryGetDocument(documentName, out Document document))
            {
                SourceText documentText = document.GetTextSynchronously(CancellationToken.None);
                using (StreamWriter writer = File.CreateText(Path.Combine(this.GetWorkingDirectory(), documentName)))
                {
                    documentText.Write(writer);
                }
            }
          
        }

        public async Task<string> GetBufferAsync(string documentName)
        {
            if (TryGetDocument(documentName, out Document document))
            {
                var sourceText = await document.GetTextAsync();
                return sourceText.ToString();
            }
            throw new ArgumentException($"{documentName} is not open in workspace");
        }

        public async Task UpdateBufferAsync(UpdateBufferRequest updateBufferRequest)
        {
            if (TryGetDocument(updateBufferRequest.FileName, out Document document))
            {
                var solution = workspace.CurrentSolution;
                var sourceText = SourceText.From(updateBufferRequest.Buffer,encoding:System.Text.Encoding.UTF8);
                solution = solution.WithDocumentText(document.Id, sourceText);
                workspace.TryApplyChanges(solution);
            }
            await Task.CompletedTask;
        }

        public async Task ChangeBufferAsync(ChangeBufferRequest changeBufferRequest)
        {
            if (TryGetDocument(changeBufferRequest.FileName, out Document document))
            {
                var solution = workspace.CurrentSolution;
                var sourceText = await document.GetTextAsync();
                var startOffset = sourceText.Lines.GetPosition(new LinePosition(changeBufferRequest.StartLine, changeBufferRequest.StartColumn));
                var endOffset = sourceText.Lines.GetPosition(new LinePosition(changeBufferRequest.EndLine, changeBufferRequest.EndColumn));
                sourceText = sourceText.WithChanges(new[] {
                        new TextChange(new TextSpan(startOffset, endOffset - startOffset), changeBufferRequest.NewText)
                    });
                solution = solution.WithDocumentText(document.Id, sourceText);                
                workspace.TryApplyChanges(solution);
            }
        }

       
        public void WithAssemblyReferences(string[] assemblyReferences)
        {
            Guard.Argument(assemblyReferences).NotNull();          
            int index = 0;
            foreach (var reference in assemblyReferences)
            {
                if(!Path.IsPathRooted(reference))
                {
                    this.additionalReferences.Add(Assembly.LoadFrom(reference));
                }
                else
                {
                    this.additionalReferences.Add(Assembly.LoadFile(reference));
                }
                index++;
            }

        }

        public void WithAssemblyReferences(Assembly[] assemblyReferences)
        {
            Guard.Argument(assemblyReferences).NotNull();
            this.additionalReferences.AddRange(assemblyReferences);
        }
    

        #endregion IWorkspaceManager

        #region Host Services

        protected HostServices HostServices { get; private set; }

        protected  CompositionHost compositionContext;

        protected IDocumentationProviderService documentationProviderService;

        public Func<string, DocumentationProvider> DocumentationProviderFactory => documentationProviderService.GetDocumentationProvider;

        void InitializeCompositionHost()
        {
            var assemblies = GetDefaultCompositionAssemblies();          

            var partTypes = assemblies
                .SelectMany(x => x.DefinedTypes)
                .Select(x => x.AsType()).Distinct();

            compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                .CreateContainer();

            HostServices = MefHostServices.Create(compositionContext);

            documentationProviderService = GetService<IDocumentationProviderService>();

        }

        public TService GetService<TService>()
        {
            return compositionContext.GetExport<TService>();
        }

        internal static readonly ImmutableArray<Assembly> DefaultCompositionAssemblies =
            ImmutableArray.Create(
            // Microsoft.CodeAnalysis.Workspaces
            typeof(WorkspacesResources).GetTypeInfo().Assembly,
            // Microsoft.CodeAnalysis.CSharp.Workspaces
            typeof(CSharpWorkspaceResources).GetTypeInfo().Assembly,
            // Microsoft.CodeAnalysis.Features
            typeof(FeaturesResources).GetTypeInfo().Assembly,
            // Microsoft.CodeAnalysis.CSharp.Features
            typeof(CSharpFeaturesResources).GetTypeInfo().Assembly,
            // this
            typeof(AdhocWorkspaceManager).GetTypeInfo().Assembly);

        protected virtual IEnumerable<Assembly> GetDefaultCompositionAssemblies()
        {
            return DefaultCompositionAssemblies;
        }

        #endregion Host Services

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                this.workspace?.Dispose();
                this.workspace = null;
            }
        }
      
        #endregion IDisposable
    }
}
