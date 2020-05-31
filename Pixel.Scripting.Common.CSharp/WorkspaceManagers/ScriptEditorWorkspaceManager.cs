using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Editor.Core.Contracts;
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
        private Type globalsType = default;
        private List<string> searchPaths = new List<string>();

        public ScriptWorkSpaceManager(string workingDirectory, Type globalsType) : base(workingDirectory)
        {
            this.globalsType = globalsType ?? typeof(object);
            DiagnosticProvider.Enable(workspace, DiagnosticProvider.Options.Syntax | DiagnosticProvider.Options.ScriptSemantic);            
        }     

        protected override ProjectId CreateNewProject()
        {
            if(compilationOptions == null)
            {
                compilationOptions = CreateCompilationOptions();
            }
            ProjectId id = ProjectId.CreateNewId();

            var defaultMetaDataReferences = ProjectReferences.DesktopRefsDefault.GetReferences(DocumentationProviderFactory);
            var additionalProjectReferences = ProjectReferences.Empty.With(assemblyReferences: this.additionalReferences);
            var additionalMetaDataReferences = additionalProjectReferences.GetReferences(DocumentationProviderFactory);
            var argumentsProjectReferences = new List<ProjectReference>();

            //During initialization of ScriptEditorFactory from ProjectManager, We add a project initially that contains Arguments.csx file.
            //We want this project to be available to all other script editor projects so that intelli-sense for any arguments defined inside 
            //this script file is available in subsequent scripts.
            if (workspace.CurrentSolution.Projects.Count() >= 1)
            {
                argumentsProjectReferences.Add(new ProjectReference(workspace.CurrentSolution.Projects.First().Id));
            }

            var scriptProjectInfo = ProjectInfo.Create(id, VersionStamp.Create(), Guid.NewGuid().ToString(),
              Guid.NewGuid().ToString(), LanguageNames.CSharp, projectReferences : argumentsProjectReferences, isSubmission: true, hostObjectType: globalsType)
               .WithMetadataReferences(new[]
               {
                    MetadataReference.CreateFromFile(globalsType.Assembly.Location)
               }
               .Concat(defaultMetaDataReferences).Concat(additionalMetaDataReferences)
               )            
               .WithCompilationOptions(compilationOptions);

            workspace.AddProject(scriptProjectInfo);

            return id;
        }

        /// <inheritdoc/>
        public override void AddDocument(string targetDocument, string initialContent)
        {
            Guard.Argument(targetDocument).NotEmpty().NotNull();
            Guard.Argument(initialContent).NotNull();   

        
            ProjectId projectId = CreateNewProject();

            var scriptDocumentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(projectId), Path.GetFileName(targetDocument),
            sourceCodeKind: SourceCodeKind.Script,
            filePath: Path.Combine(this.GetWorkingDirectory(), targetDocument),
            loader: TextLoader.From(TextAndVersion.Create(SourceText.From(initialContent), VersionStamp.Create())));           

            var updatedSolution = workspace.CurrentSolution.AddDocument(scriptDocumentInfo);
            workspace.TryApplyChanges(updatedSolution);

            //return scriptDocumentInfo.Id;
        }

        /// <summary>
        /// Close document and remove owner project from workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <returns></returns>
        public override bool TryCloseDocument(string targetDocument)
        {
            if (TryGetDocument(targetDocument, out Document document))
            {
                if (workspace.IsDocumentOpen(document.Id))
                {
                    workspace.CloseDocument(document.Id);
                    //We want to remove projects other then the first project when closing editor
                    if(document.Project.Id != workspace.CurrentSolution.ProjectIds.First())
                    {
                        var updatedSolution = workspace.CurrentSolution.RemoveProject(document.Project.Id);
                        workspace.TryApplyChanges(updatedSolution);
                    }                   
                    return true;
                }
            }
            return false;
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

        public void WithSearchPaths(params string[] searchPaths)
        {
            this.searchPaths = this.searchPaths.Union(searchPaths).ToList<string>();
            this.compilationOptions = CreateCompilationOptions();
        }
    }
}
