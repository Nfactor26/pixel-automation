using Microsoft.CodeAnalysis.Diagnostics;
using Pixel.Script.Editor.Services.CSharp.Helpers;
using Pixel.Script.Editor.Services.CSharp.Highlight;
using Pixel.Script.Editor.Services.CSharp.Signature;
using Pixel.Script.Editor.Services.CSharp.TypeLookup;
using Pixel.Scripting.Common.CSharp.Diagnostics;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Editor.Core.Models;
using Pixel.Scripting.Editor.Core.Models.CodeActions;
using Pixel.Scripting.Editor.Core.Models.CodeFormat;
using Pixel.Scripting.Editor.Core.Models.Completions;
using Pixel.Scripting.Editor.Core.Models.Diagnostics;
using Pixel.Scripting.Editor.Core.Models.Highlights;
using Pixel.Scripting.Editor.Core.Models.Signatures;
using Pixel.Scripting.Editor.Core.Models.TypeLookup;
using Pixel.Scripting.Editor.Services.CodeActions;
using Pixel.Scripting.Editor.Services.Completion;
using Pixel.Scripting.Editor.Services.CSharp;
using Pixel.Scripting.Editor.Services.CSharp.Formatting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Script.Editor.Services.CSharp
{
    public class EditorService : IEditorService
    {
        private readonly ILogger logger = Log.ForContext<EditorService>();

        private readonly ICodeGenerator codeGenerator;
        private WorkspaceOptions editorOptions;
        private AdhocWorkspaceManager workspaceManager;
        private IntellisenseService intelliSenseService;
        private HighlightService highlightService;
        private SignatureHelpService signatureHelpService;
        private TypeLookupService typeLookupService;
        private CodeFormatService codeFormatService;
        private FormatRangeService formatRangeService;
        private FormatAfterKeystrokeService formatAfterKeyStrokeService;
        private CommentService commentService;
        private GetCodeActionsService codeActionsService;
        private RunCodeActionService runCodeActionService;

        public event EventHandler<WorkspaceChangedEventArgs> WorkspaceChanged;

        public IObservable<DiagnosticResultEx> DiagnosticsUpdated { get; private set; }

        public EditorService(ICodeGenerator codeGenerator)
        {
            this.codeGenerator = codeGenerator;
        }

        public IWorkspaceManager GetWorkspaceManager()
        {
            return this.workspaceManager;
        }

        public void Initialize(WorkspaceOptions editorOptions)
        {
            ClearState();
            
            this.editorOptions = editorOptions;
            switch(editorOptions.WorkspaceType)
            {
                case WorkspaceType.Code:
                    this.workspaceManager = new CodeWorkspaceManager(editorOptions.WorkingDirectory);
                    break;
                case WorkspaceType.Script:
                    this.workspaceManager = new ScriptWorkSpaceManager(editorOptions.WorkingDirectory);
                    break;
            }           

            this.workspaceManager.WithAssemblyReferences(editorOptions.AssemblyReferences ?? Array.Empty<string>());

            var formattingOptions = new FormattingOptions();
            this.intelliSenseService = new IntellisenseService(workspaceManager, formattingOptions);
            this.highlightService = new HighlightService(workspaceManager);
            this.signatureHelpService = new SignatureHelpService(workspaceManager);
            this.typeLookupService = new TypeLookupService(workspaceManager, formattingOptions);
            this.codeFormatService = new CodeFormatService(workspaceManager);
            this.formatRangeService = new FormatRangeService(workspaceManager);
            this.formatAfterKeyStrokeService = new FormatAfterKeystrokeService(workspaceManager);
            this.commentService = new CommentService(workspaceManager);

            if(editorOptions.EnableDiagnostics)
            {
                var diagnosticService = workspaceManager.GetService<Microsoft.CodeAnalysis.Diagnostics.IDiagnosticService>();
                var diagnosticsEventObservable = System.Reactive.Linq.Observable.FromEventPattern<DiagnosticsUpdatedArgs>(diagnosticService, nameof(diagnosticService.DiagnosticsUpdated));
                var diagnosticsMessageObservable = diagnosticsEventObservable.Select(d =>
                {
                    var diagnosticEventData = d.EventArgs;

                    if (diagnosticEventData.Kind != DiagnosticsUpdatedKind.DiagnosticsCreated)
                    {
                        return null;
                    }

                   
                    var document = workspaceManager.GetDocumentById(diagnosticEventData.DocumentId);
                    var project = diagnosticEventData.Solution.GetProject(diagnosticEventData.ProjectId);
                    DiagnosticResultEx diagnosticResult = new DiagnosticResultEx()
                    {
                        Id = diagnosticEventData.Id,
                        FileName = document.Name,
                        ProjectName = project.Name
                    };                   
                    List<DiagnosticLocation> quickFixes = new List<DiagnosticLocation>();
                    foreach (var diagnosticData in diagnosticEventData.Diagnostics)
                    {
                        var diagnostic = diagnosticData.ToDiagnosticAsync(project, CancellationToken.None).Result;
                        var diagnosticLocation = diagnostic.ToDiagnosticLocation();
                        quickFixes.Add(diagnosticLocation);
                    }
                    diagnosticResult.QuickFixes = quickFixes;
                    return diagnosticResult;
                });

                DiagnosticsUpdated = diagnosticsMessageObservable.Where(d => d != null);


                if (editorOptions.EnableCodeActions)
                {
                    this.codeActionsService = new GetCodeActionsService(this.workspaceManager, new[]
                    {
                    new RoslynCodeActionProvider()
                    },
                    diagnosticService, new CachingCodeFixProviderForProjects());

                    this.runCodeActionService = new RunCodeActionService(this.workspaceManager,
                    new CodeActionHelper(), new[]
                    {
                     new RoslynCodeActionProvider()
                    },
                    diagnosticService, new CachingCodeFixProviderForProjects());
                }

            }

            OnWorkspaceChanged(new WorkspaceChangedEventArgs());

            logger.Information($"{nameof(EditorService)} of type {editorOptions.WorkspaceType} is initialized now.");
        }

        public void SwitchToDirectory(string directory)
        {
            if(string.IsNullOrEmpty(directory))
            {
                workspaceManager.SetCurrentDirectory(null);
            }
            workspaceManager.SetCurrentDirectory(directory);
        }

        public IEnumerable<string> GetAvailableProjects()
        {
            return this.workspaceManager.GetAllProjects().ToList();
        }
       
        public string GetDefaultNameSpace(string projectName)
        {
            return this.workspaceManager.GetDefaultNameSpace(projectName);
        }


        public IEnumerable<string> GetAvailableDocuments()
        {
            string workingDirectory = workspaceManager.GetWorkingDirectory();
            foreach(var file in Directory.GetFiles(workingDirectory, "*.cs"))
            {
                yield return Path.GetFileName(file);
            }
            yield break;
        }

        public bool IsFeatureEnabled(EditorFeature feature)
        {
            switch(feature)
            {
                case EditorFeature.CodeActions:
                    return editorOptions.EnableCodeActions;
                case EditorFeature.Diagnostics:
                    return editorOptions.EnableDiagnostics;
                case EditorFeature.Documentation:
                    return editorOptions.EnableDocumentation;
                default:
                    throw new ArgumentException($"{feature} is not supported");
            }
        }

        protected virtual void OnWorkspaceChanged(WorkspaceChangedEventArgs w)
        {
            this.WorkspaceChanged?.Invoke(this, w);
        }

        private void ClearState()
        {
            if (this.workspaceManager != null)
            {                
                this.workspaceManager.Dispose();
                logger.Information("Disposed workspace manager for editor service");
            }
        }

        /// <inheritdoc/>
        public void CreateFileIfNotExists(string targetFile, string initialContent)
        {          
            if (Path.IsPathRooted(targetFile))
            {
                throw new ArgumentException($"{targetFile} must be  relative path to working directory.");
            }

            string documentLocation = Path.Combine(workspaceManager.GetWorkingDirectory(), targetFile);
            if (!File.Exists(documentLocation))
            {
                File.WriteAllText(documentLocation, initialContent);
            }
        }


        /// <inheritdoc/>
        public string GetFileContentFromDisk(string targetFile)
        {
            string documentLocation = Path.Combine(workspaceManager.GetWorkingDirectory(), targetFile);
            if(File.Exists(documentLocation))
            {
                return File.ReadAllText(documentLocation);
            }
            throw new FileNotFoundException($"{targetFile} doesn't exist at location {documentLocation}");
        }

        /// <inheritdoc/>
        public bool HasDocument(string targetDocument, string ownerProject)
        {
            return this.workspaceManager.HasDocument(targetDocument, ownerProject);
        }

        /// <inheritdoc/>
        public void AddDocument(string targetDocument, string addToProject, string documentContent)
        {
            this.workspaceManager.AddDocument(targetDocument, addToProject, documentContent);
        }


        public string CreateDocument(string className, string nameSpace, IEnumerable<string> imports)
        {
            var classGenerator  = this.codeGenerator.CreateClassGenerator(className, nameSpace, imports);
            return classGenerator.GetGeneratedCode();
        }


        /// <inheritdoc/>
        public void RemoveDocument(string targetDocument, string removeFromProject)
        {
            if(this.workspaceManager.TryRemoveDocument(targetDocument, removeFromProject))
            {
                string documentLocation = Path.Combine(workspaceManager.GetWorkingDirectory(), targetDocument);
                File.Delete(documentLocation);
                return;
               
            }
            throw new Exception($"Failed to remove {targetDocument} from workspace");
        }

        /// <inheritdoc/>
        public void SetContent(string targetDocument, string ownerProject, string documentContent)
        {
            _ =  UpdateBufferAsync(new UpdateBufferRequest() { FileName = targetDocument, Buffer = documentContent });           
        }

        /// <inheritdoc/>
        public void SaveDocument(string targetDocument, string ownerProject)
        {
            this.workspaceManager.SaveDocument(targetDocument, ownerProject);
        }

        /// <inheritdoc/>
        public bool TryOpenDocument(string targetDocument, string ownerProject)
        {
            return this.workspaceManager.TryOpenDocument(targetDocument, ownerProject);
        }

        /// <inheritdoc/>
        public bool TryCloseDocument(string targetDocument, string ownerProject)
        {
            return this.workspaceManager.TryCloseDocument(targetDocument, ownerProject);
        }

        public async Task UpdateBufferAsync(UpdateBufferRequest request)
        {
           await this.workspaceManager.UpdateBufferAsync(request);
        }

        public async Task ChangeBufferAsync(ChangeBufferRequest request)
        {
            await this.workspaceManager.ChangeBufferAsync(request);
        }


        public async Task<HighlightResponse> GetHighlightsAsync(HighlightRequest request)
        {
            return await this.highlightService.GetHighlights(request);
        }

        public async Task<TypeLookupResponse> GetTypeDescriptionAsync(TypeLookupRequest request)
        {
            return await this.typeLookupService.GetTypeDescriptionAsync(request);
        }

        public async Task<IEnumerable<AutoCompleteResponse>> GetCompletionsAsync(AutoCompleteRequest request)
        {
            return await this.intelliSenseService.GetCompletions(request);
        }

        public async Task<SignatureHelpResponse> GetSignaturesAsync(SignatureHelpRequest request)
        {
            return await this.signatureHelpService.GetSignatures(request);
        }

        public async Task<CodeFormatResponse> GetFormattedCodeAsync(CodeFormatRequest formatDocumentRequest)
        {
            return await this.codeFormatService.GetFormattedCodeAsync(formatDocumentRequest);
        }

        public async Task<FormatRangeResponse> GetFormattedRangeAsync(FormatRangeRequest formatRangeRequest)
        {
            return await this.formatRangeService.GetFormattedRangeAsync(formatRangeRequest);
        }

        public async Task<FormatRangeResponse> GetFormattedCodeAfterKeystrokeAsync(FormatAfterKeystrokeRequest formatAfterKeystrokeRequest)
        {
            return await this.formatAfterKeyStrokeService.GetFormattedCodeAfterKeystrokeAsync(formatAfterKeystrokeRequest);
        }

        public async Task<FormatRangeResponse> GetCommentedTextAsync(FormatRangeRequest formatRangeRequest)
        {
            return await this.commentService.CommentSelectionAsync(formatRangeRequest);
        }

        public async Task<FormatRangeResponse> GetUnCommentedTextAsync(FormatRangeRequest formatRangeRequest)
        {
            return await this.commentService.UncommentSelectionAsync(formatRangeRequest);
        }

        public async Task<GetCodeActionsResponse> GetCodeActionsAsync(GetCodeActionsRequest getCodeActionsRequest)
        {
            return await this.codeActionsService.GetCodeActionsAsync(getCodeActionsRequest);
        }

        public async Task<RunCodeActionResponse> RunCodeActionAsync(RunCodeActionRequest runCodeActionsRequest)
        {
            return await this.runCodeActionService.RunCodeActionAsync(runCodeActionsRequest);
        }
       
    }
}
