using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Editor.Core.Models;
using Serilog;
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
        protected ILogger logger;
       
        private string workingDirectory;
        protected AdhocWorkspace workspace = default;              
      
        protected List<Assembly> additionalReferences;

        public AdhocWorkspaceManager(string workingDirectory)
        {
            Guard.Argument(workingDirectory).NotNull().NotEmpty();

            if (!Directory.Exists(workingDirectory))
            {
                throw new ArgumentException($"{workingDirectory ?? "NULL"} doesn't exist.");
            }

            this.workingDirectory = workingDirectory;

            InitializeCompositionHost();

            workspace = new AdhocWorkspace(HostServices);
            workspace.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create()));
            this.additionalReferences = new List<Assembly>();
        }

        protected virtual CSharpCompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: ProjectReferences.DesktopRefsDefault.Imports);
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
                    if (document.Name.Equals(Path.GetFileName(documentName)))
                    {
                        project = proj;
                        return true;
                    }
                }
            }
            project = default;
            return false;
        }

        public bool HasProject(string projectName)
        {
            foreach (var project in this.workspace.CurrentSolution.Projects)
            {
                if (project.Name.Equals(projectName))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void RemoveProject(string projectName)
        {
            var project = GetProjectByName(projectName);
            var updatedSolution = workspace.CurrentSolution.RemoveProject(project.Id);
            if(!workspace.TryApplyChanges(updatedSolution))
            {
                throw new Exception($"Failed to remove project : {projectName} from workspace");
            }
            logger.Information($"Removed project {projectName}. Workspace has total {workspace.CurrentSolution.Projects.Count()} project now.");
        }


        public string GetDefaultNameSpace(string projectName)
        {
            Guard.Argument(projectName).NotNull().NotEmpty();
            var project = GetProjectByName(projectName);
            return project.DefaultNamespace ?? string.Empty;
        }

        public IEnumerable<string> GetAllProjects()
        {
            foreach (var project in this.workspace.CurrentSolution.Projects)
            {
                yield return project.Name;
            }
            yield break;
        }

        protected virtual Project GetProjectByName(string projectName)
        {
            foreach (var project in this.workspace.CurrentSolution.Projects)
            {
                if (project.Name.Equals(projectName))
                {
                    return project;
                }
            }
            throw new ArgumentException($"Project with name {projectName} doesn't exist in workspace");
        }

        public bool TryGetDocument(string documentName, out Document document)
        {
            Guard.Argument(documentName).NotNull().NotEmpty();
         
            foreach (var project in this.workspace.CurrentSolution.Projects)
            {
                foreach (var doc in project.Documents)
                {
                    if (doc.Name.Equals(Path.GetFileName(documentName)))
                    {
                        document = doc;
                        return true;
                    }
                }
            }
            document = default;
            return false;
        }

        public bool TryGetDocument(string documentName, string projectName, out Document document)
        {
            Guard.Argument(documentName).NotNull().NotEmpty();
            Guard.Argument(projectName).NotNull().NotEmpty();

            foreach (var project in this.workspace.CurrentSolution.Projects)
            {
                if(project.Name.Equals(projectName))
                {
                    foreach (var doc in project.Documents)
                    {
                        if (doc.Name.Equals(Path.GetFileName(documentName)))
                        {
                            document = doc;
                            return true;
                        }
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
            return this.workingDirectory;
        }

        public void SwitchWorkingDirectory(string workingDirectory)
        {
            Guard.Argument(workingDirectory).NotNull().NotEmpty();
            if(!Directory.Exists(workingDirectory))
            {
                throw new ArgumentException($"{workingDirectory ?? "NULL"} doesn't exist.");
            }
            this.workingDirectory = workingDirectory;
            logger.Information("WorkingDirectory was changed to {workingDirectory}", workingDirectory);
        }

        /// <inheritdoc/>
        public abstract void AddDocument(string targetDocument, string addToProject, string initialContent);

        /// <inheritdoc/>
        public bool TryRemoveDocument(string targetDocument, string removeFromProject)
        {
            if(TryGetDocument(targetDocument, removeFromProject, out Document document))
            {
                var project = document.Project;
                project = project.RemoveDocument(document.Id);
                if(workspace.TryApplyChanges(project.Solution))
                {
                    logger.Information($"Removed document {targetDocument} from project {removeFromProject}");
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public bool IsDocumentOpen(string targetDocument, string ownerProject)
        {
            if (TryGetDocument(targetDocument, ownerProject, out Document document))
            {
                return workspace.IsDocumentOpen(document.Id);               
            }
            return false;
        }

        /// <inheritdoc/>
        public bool TryOpenDocument(string targetDocument, string ownerProject)
        {
            if (TryGetDocument(targetDocument, ownerProject, out Document document))
            {
                if(!workspace.IsDocumentOpen(document.Id))
                {
                    workspace.OpenDocument(document.Id);
                    logger.Information($"Document {targetDocument} from project {ownerProject} is open now");
                    return true;
                }               
            }
            return false;        
        }

        /// <summary>
        /// Close document in workspace and remove associated project.
        /// By default, one document per project is supported.
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <returns></returns>
        public virtual bool TryCloseDocument(string targetDocument, string ownerProject)
        {
            if (TryGetDocument(targetDocument, ownerProject, out Document document))
            {
                if (workspace.IsDocumentOpen(document.Id))
                {
                    workspace.CloseDocument(document.Id);
                    logger.Information($"Document {targetDocument} from project {ownerProject} is closed now");
                    return true;
                }               
            }
            return false;           
        }

        /// <inheritdoc/>
        public bool HasDocument(string targetDocument, string ownerProject)
        {
            return TryGetDocument(targetDocument, ownerProject, out Document _ );
        }

        /// <inheritdoc/>
        public void SaveDocument(string targetDocument, string ownerProject)
        {
            if (TryGetDocument(targetDocument, ownerProject, out Document document))
            {
                SourceText documentText = document.GetTextSynchronously(CancellationToken.None);
                using (StreamWriter writer = File.CreateText(Path.Combine(this.GetWorkingDirectory(), targetDocument)))
                {
                    documentText.Write(writer);
                }
                logger.Information($"Document {targetDocument} from project {ownerProject} has been saved.");
            }

        }

        /// <inheritdoc/>
        public async Task<string> GetBufferAsync(string targetDocument, string ownerProject)
        {
            if (TryGetDocument(targetDocument, ownerProject, out Document document))
            {
                var sourceText = await document.GetTextAsync();
                return sourceText.ToString();
            }
            throw new ArgumentException($"{targetDocument} is not open in workspace");
        }

        public async Task UpdateBufferAsync(UpdateBufferRequest updateBufferRequest)
        {
            if (TryGetDocument(updateBufferRequest.FileName, out Document document))
            {
                var solution = workspace.CurrentSolution;
                var sourceText = SourceText.From(updateBufferRequest.Buffer, encoding:System.Text.Encoding.UTF8);
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
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                this.workspace?.Dispose();
                this.workspace = null;
                logger.Information($"Workspace is disposed now.");

            }
        }       

        #endregion IDisposable
    }
}
