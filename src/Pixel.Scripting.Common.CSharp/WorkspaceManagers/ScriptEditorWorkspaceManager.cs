using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Pixel.Automation.Reference.Manager;
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
        private CachedScriptMetadataResolver metaDataReferenceResolver;
        private CSharpCompilationOptions scriptCompilationOptions;
        private HashSet<string> imports = new HashSet<string>();

        public ScriptWorkSpaceManager(string workingDirectory) : base(workingDirectory)
        {                   
            logger = Log.ForContext<ScriptWorkSpaceManager>();
        }     

        public  void AddProject(string projectName, IEnumerable<string> projectReferences, Type globalsType)
        {
            Guard.Argument(projectName).NotNull().NotEmpty();
            Guard.Argument(projectReferences).NotNull();         
            Guard.Argument(globalsType).NotNull();

            if (scriptCompilationOptions == null)
            {
                scriptCompilationOptions = CreateCompilationOptions();
            }
            ProjectId id = ProjectId.CreateNewId();

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
               .Concat(this.additionalReferences)
               )
               .WithCompilationOptions(scriptCompilationOptions);

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
     
        protected override CSharpCompilationOptions CreateCompilationOptions()
        {
            this.metaDataReferenceResolver = CreateMetaDataResolver();
            
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: AssemblyReferences.DefaultNamespaces.Imports.Union(this.imports));           
            compilationOptions = compilationOptions.WithMetadataReferenceResolver(metaDataReferenceResolver);
           
            //SourceFileResolver is required so that #load directive can be used in script
            this.scriptCompilationOptions = compilationOptions.WithSourceReferenceResolver(new SourceFileResolver(this.searchPaths, this.GetWorkingDirectory()));
            return compilationOptions;
        }

        private CachedScriptMetadataResolver CreateMetaDataResolver()
        {            
            var metaDataResolver = ScriptMetadataResolver.Default;
            metaDataResolver = metaDataResolver.WithBaseDirectory(Environment.CurrentDirectory);           
            metaDataResolver = metaDataResolver.WithSearchPaths(this.searchPaths);

            return new CachedScriptMetadataResolver(metaDataResolver, useCache: true);
        }

        public void AddSearchPaths(params string[] searchPaths)
        {
            this.searchPaths = this.searchPaths.Union(searchPaths).ToList();
            UpdateCompilationOptions();
        }     

        public void RemoveSearchPaths(params string[] searchPaths)
        {
            this.searchPaths = this.searchPaths.Except(searchPaths).ToList();
            UpdateCompilationOptions();
        }

        public void AddImports(params string[] imports)
        {
            foreach(var import in imports)
            {
                this.imports.Add(import);
            }
        }

        void UpdateCompilationOptions()
        {
            var metaDataResolver = ScriptMetadataResolver.Default;
            metaDataResolver = metaDataResolver.WithBaseDirectory(Environment.CurrentDirectory);
            metaDataResolver = metaDataResolver.WithSearchPaths(this.searchPaths);

            this.metaDataReferenceResolver = this.metaDataReferenceResolver?.WithScriptMetaDataResolver(metaDataResolver);
            this.scriptCompilationOptions = this.scriptCompilationOptions?.WithMetadataReferenceResolver(this.metaDataReferenceResolver);
        }
    }
}
