using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixel.Scripting.Common.CSharp.WorkspaceManagers
{
    /// <summary>
    /// CodeWorkspaceManager wraps an AdhocWorkspace and supports .cs files
    /// A project can have more then one document.
    /// </summary>
    public class CodeWorkspaceManager : AdhocWorkspaceManager , ICodeWorkspaceManager
    {
        public CodeWorkspaceManager(string workingDirectory) : base(workingDirectory)
        {
            Guard.Argument(workingDirectory).NotEmpty().NotNull();
            DiagnosticProvider.Enable(workspace, DiagnosticProvider.Options.Syntax | DiagnosticProvider.Options.Semantic);
            logger = Log.ForContext<CodeWorkspaceManager>();
        }    

        public void AddProject(string projectName, string defaultNameSpace, IEnumerable<string> projectReferences)
        {
            Guard.Argument(projectName).NotNull().NotEmpty();
            Guard.Argument(defaultNameSpace).NotNull().NotEmpty();
            Guard.Argument(projectReferences).NotNull();

            var compilationOptions = CreateCompilationOptions();
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

            var projectInfo = ProjectInfo.Create(id, VersionStamp.Create(), projectName,
                Guid.NewGuid().ToString(), LanguageNames.CSharp, projectReferences: otherProjectReferences, isSubmission: false)
               .WithMetadataReferences(defaultMetaDataReferences.Concat(additionalMetaDataReferences))
               .WithCompilationOptions(compilationOptions);
            projectInfo = projectInfo.WithDefaultNamespace(defaultNameSpace);

            workspace.AddProject(projectInfo);

            logger.Information($"Added new project : {projectName}. Workspace has total {workspace.CurrentSolution.Projects.Count()} project now.");
        }

        public override void AddDocument(string targetDocument, string addToProject, string initialContent)
        {
            Guard.Argument(targetDocument).NotNull().NotEmpty();
            Guard.Argument(initialContent).NotNull();
            Guard.Argument(addToProject).NotNull().NotEmpty();
      
            var project = GetProjectByName(addToProject);          

            var documentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(project.Id), Path.GetFileName(targetDocument),
            sourceCodeKind: SourceCodeKind.Regular,
            filePath: Path.Combine(this.GetWorkingDirectory(), targetDocument),
            loader: TextLoader.From(TextAndVersion.Create(SourceText.From(initialContent, encoding: System.Text.Encoding.UTF8), VersionStamp.Create())));

            var updatedSolution = workspace.CurrentSolution.AddDocument(documentInfo);
            workspace.TryApplyChanges(updatedSolution);

            logger.Information($"Added document {targetDocument} to project {addToProject}");
        }

        /// <summary>
        /// Close document in workspace.
        /// </summary>
        /// <param name="targetDocument">Relative path of document to working directory</param>
        /// <returns></returns>
        public override bool TryCloseDocument(string targetDocument, string ownerProject)
        {
            Guard.Argument(targetDocument).NotNull().NotEmpty();         
            Guard.Argument(ownerProject).NotNull().NotEmpty();

            if (TryGetDocument(targetDocument, ownerProject, out Document document))
            {
                if (workspace.IsDocumentOpen(document.Id))
                {
                    workspace.CloseDocument(document.Id);
                    logger.Information($"Document {targetDocument} in project {ownerProject} has been closed");
                    return true;
                }
            }
            return false;
        }         

        public CompilationResult CompileProject(string projectName, string outputAssemblyName)
        {
            Guard.Argument(projectName).NotNull().NotEmpty();
            Guard.Argument(outputAssemblyName).NotNull().NotEmpty();

            var project = GetProjectByName(projectName);
            Compilation projectCompilation = project.GetCompilationAsync(new System.Threading.CancellationToken()).Result;

            var assemblyStream = new MemoryStream();
            var pdbStream = new MemoryStream();
            //var compilationResult = projectCompilation.Emit(assemblyStream, pdbStream: pdbStream, options: new EmitOptions().
            //           WithDebugInformationFormat(DebugInformationFormat.Pdb).WithOutputNameOverride(outputAssemblyName));

            var compilationOptions = projectCompilation.Options;
            //compilationOptions = compilationOptions.WithOptimizationLevel(OptimizationLevel.Debug);           

            EmitResult compilationResult = CSharpCompilation
                .Create(outputAssemblyName, projectCompilation.SyntaxTrees, projectCompilation.References, options: compilationOptions as CSharpCompilationOptions)
                .Emit(assemblyStream, pdbStream);

            if (!compilationResult.Success)
            {
                var errors = string.Join(Environment.NewLine, compilationResult.Diagnostics.Select(x => x.ToString()));
                throw new CompilationErrorException($"Workspace couldn't be compiled.{errors}", compilationResult.Diagnostics);
            }
            logger.Information($"Project {projectName} was compiled successfully.");
            return new CompilationResult(outputAssemblyName, assemblyStream, pdbStream);
        }

    }
}
