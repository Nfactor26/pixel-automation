using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
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
            //CreateNewProject();
        }

        protected override ProjectId CreateNewProject()
        {
            var compilationOptions = CreateCompilationOptions();
            ProjectId id = ProjectId.CreateNewId();

            var defaultMetaDataReferences = ProjectReferences.DesktopRefsDefault.GetReferences(DocumentationProviderFactory);
            var additionalProjectReferences = ProjectReferences.Empty.With(assemblyReferences: this.additionalReferences);
            var additionalMetaDataReferences = additionalProjectReferences.GetReferences(DocumentationProviderFactory);

            var projectInfo = ProjectInfo.Create(id, VersionStamp.Create(), Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(), LanguageNames.CSharp, isSubmission: false)
               .WithMetadataReferences(defaultMetaDataReferences.Concat(additionalMetaDataReferences))
               .WithCompilationOptions(compilationOptions);

            workspace.AddProject(projectInfo);
            return id;
        }

        public override void AddDocument(string targetDocument, string initialContent)
        {
            Guard.Argument(targetDocument).NotEmpty().NotNull();
            Guard.Argument(initialContent).NotNull();

            //There is  only one project created at initialization
            //All code file belongs to this project
            ProjectId projectId = workspace.CurrentSolution.ProjectIds.FirstOrDefault() ?? CreateNewProject();
           

            var scriptDocumentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(projectId), Path.GetFileName(targetDocument),
            sourceCodeKind: SourceCodeKind.Regular,
            filePath: Path.Combine(this.GetWorkingDirectory(), targetDocument),
            loader: TextLoader.From(TextAndVersion.Create(SourceText.From(initialContent, encoding: System.Text.Encoding.UTF8), VersionStamp.Create())));

            var updatedSolution = workspace.CurrentSolution.AddDocument(scriptDocumentInfo);
            workspace.TryApplyChanges(updatedSolution);

            //return scriptDocumentInfo.Id;
        }

        /// <summary>
        /// Close document in workspace.
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
                    return true;
                }
            }
            return false;
        }    

      
        public CompilationResult CompileProject(string outputAssemblyName)
        {          
            var project = workspace.CurrentSolution.Projects.FirstOrDefault();
            Compilation projectCompilation = project.GetCompilationAsync(new System.Threading.CancellationToken()).Result;

            var assemblyStream = new MemoryStream();
            var pdbStream = new MemoryStream();
            //var compilationResult = projectCompilation.Emit(assemblyStream, pdbStream: pdbStream, options: new EmitOptions().
            //           WithDebugInformationFormat(DebugInformationFormat.Pdb).WithOutputNameOverride(outputAssemblyName));

            var compilationOptions = projectCompilation.Options;
            //compilationOptions = compilationOptions.WithOptimizationLevel(OptimizationLevel.Debug);           

            EmitResult compilationResult = CSharpCompilation
                .Create(outputAssemblyName, projectCompilation.SyntaxTrees, projectCompilation.References, options : compilationOptions as CSharpCompilationOptions)
                .Emit(assemblyStream,pdbStream);

            if (!compilationResult.Success)
            {
                var errors = string.Join(Environment.NewLine, compilationResult.Diagnostics.Select(x => x.ToString()));
                throw new CompilationErrorException($"Workspace couldn't be compiled.{errors}", compilationResult.Diagnostics);
            }
            return new CompilationResult(outputAssemblyName, assemblyStream, pdbStream);
        }

    }
}
