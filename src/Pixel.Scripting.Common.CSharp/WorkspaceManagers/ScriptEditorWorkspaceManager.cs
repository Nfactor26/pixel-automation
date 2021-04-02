using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Pixel.Scripting.Common.CSharp.WorkspaceManagers
{
    /// <summary>
    /// ScriptWorkSpaceManager wraps an AdhocWorkspace and supports .csx (script) files
    /// Each script file is added to a new project. Removing script file will also remove that project.
    /// </summary>
    public class ScriptWorkSpaceManager : AdhocWorkspaceManager , IScriptWorkspaceManager
    {      
        private List<string> searchPaths = new List<string>();
        public ScriptWorkSpaceManager(string workingDirectory) : base(workingDirectory)
        {         
            DiagnosticProvider.Enable(workspace, DiagnosticProvider.Options.Syntax | DiagnosticProvider.Options.ScriptSemantic);
            logger = Log.ForContext<ScriptWorkSpaceManager>();
        }     

        public  void AddProject(string projectName, IEnumerable<string> projectReferences, Type globalsType)
        {
            Guard.Argument(projectName).NotNull().NotEmpty();
            Guard.Argument(projectReferences).NotNull();         
            Guard.Argument(globalsType).NotNull();

            if (compilationOptions == null)
            {
                compilationOptions = CreateCompilationOptions();
            }
            ProjectId id = ProjectId.CreateNewId();

            var defaultMetaDataReferences = ProjectReferences.DesktopRefsDefault.GetReferences(DocumentationProviderFactory);
            var additionalProjectReferences = ProjectReferences.Empty.With(assemblyReferences: this.additionalReferences);
            var additionalMetaDataReferences = additionalProjectReferences.GetReferences(DocumentationProviderFactory);
       
            var otherProjectReferences = new List<ProjectReference>();
            foreach (var reference in projectReferences)
            {
                var project = GetProjectByName(reference);
                otherProjectReferences.Add(new ProjectReference(project.Id));
            }

            var scriptProjectInfo = ProjectInfo.Create(id, VersionStamp.Create(), projectName,
              Guid.NewGuid().ToString(), LanguageNames.CSharp, projectReferences: otherProjectReferences, isSubmission: true, hostObjectType: globalsType)
               .WithMetadataReferences(new[]
               {
                    MetadataReference.CreateFromFile(globalsType.Assembly.Location)
               }
               .Concat(defaultMetaDataReferences).Concat(additionalMetaDataReferences)
               )
               .WithCompilationOptions(compilationOptions);

            workspace.AddProject(scriptProjectInfo);

            logger.Information($"Added new project : {projectName}. Workspace has total {workspace.CurrentSolution.Projects.Count()} project now.");

        }

        /// <inheritdoc/>
        public override void AddDocument(string targetDocument, string addToProject, string initialContent)
        {
            Guard.Argument(targetDocument).NotEmpty().NotNull();
            Guard.Argument(initialContent).NotNull();


            var project = GetProjectByName(addToProject);
            if(project.Documents.Any())
            {
                throw new InvalidOperationException($"{project.Id} already has a document : {project.Documents.First().Name}. Scripting projects can contain only one document at a time.");
            }

            var scriptDocumentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(project.Id), Path.GetFileName(targetDocument),
            sourceCodeKind: SourceCodeKind.Script,
            filePath: Path.Combine(this.GetWorkingDirectory(), targetDocument),
            loader: TextLoader.From(TextAndVersion.Create(SourceText.From(initialContent), VersionStamp.Create())));           

            var updatedSolution = workspace.CurrentSolution.AddDocument(scriptDocumentInfo);
            workspace.TryApplyChanges(updatedSolution);

            logger.Information($"Added document {targetDocument} to project {addToProject}");
        }              

        private CSharpCompilationOptions compilationOptions = default;

        protected override CSharpCompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: ProjectReferences.DesktopDefault.Imports);
            compilationOptions = compilationOptions.WithMetadataReferenceResolver(CreateMetaDataResolver());
            //SourceFileResolver is required so that #load directive can be used in script
            compilationOptions = compilationOptions.WithSourceReferenceResolver(new SourceFileResolver(this.searchPaths, this.GetWorkingDirectory()));
            return compilationOptions;
        }

        private MetadataReferenceResolver CreateMetaDataResolver()
        {
            var scriptEnvironmentService = workspace.Services.GetService<IScriptEnvironmentService>();          
            var metaDataResolver = ScriptMetadataResolver.Default;
            metaDataResolver = metaDataResolver.WithBaseDirectory(scriptEnvironmentService.BaseDirectory);
            var scriptReferencesLocation = ImmutableArray<string>.Empty;
            scriptReferencesLocation = scriptReferencesLocation.AddRange(scriptEnvironmentService.MetadataReferenceSearchPaths);
            scriptReferencesLocation = scriptReferencesLocation.AddRange(this.searchPaths);
            metaDataResolver = metaDataResolver.WithSearchPaths(scriptReferencesLocation);
          
            return new CachedScriptMetadataResolver(metaDataResolver, useCache: true);
        }

        public void AddSearchPaths(params string[] searchPaths)
        {
            this.searchPaths = this.searchPaths.Union(searchPaths).ToList();
            this.compilationOptions = CreateCompilationOptions();
        }

        public void RemoveSearchPaths(params string[] searchPaths)
        {
            this.searchPaths = this.searchPaths.Except(searchPaths).ToList();
            this.compilationOptions = CreateCompilationOptions();
        }
    }
}
