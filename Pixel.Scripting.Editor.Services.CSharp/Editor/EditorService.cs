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
        private EditorOptions editorOptions;
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

        public EditorService()
        {
            
        }

        public IWorkspaceManager GetWorkspaceManager()
        {
            return this.workspaceManager;
        }

        public void Initialize(EditorOptions editorOptions, Type globalsType)
        {
            ClearState();
            
            this.editorOptions = editorOptions;
            if (globalsType == null)
            {
                this.workspaceManager = new CodeWorkspaceManager(editorOptions.WorkingDirectory);
            }
            else
            {
                this.workspaceManager = new ScriptWorkSpaceManager(editorOptions.WorkingDirectory, globalsType);
            }

            this.workspaceManager.WithAssemblyReferences(editorOptions.EditorReferences ?? Array.Empty<string>());

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

                    DiagnosticResultEx diagnosticResult = new DiagnosticResultEx();
                    var document = workspaceManager.GetDocumentById(diagnosticEventData.DocumentId);
                    var project = diagnosticEventData.Solution.GetProject(diagnosticEventData.ProjectId);
                    diagnosticResult.Id = diagnosticEventData.Id;
                    diagnosticResult.FileName = document.Name;
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
        }

        public void SwitchToDirectory(string directory)
        {
            if(string.IsNullOrEmpty(directory))
            {
                workspaceManager.SetCurrentDirectory(null);
            }

            //if(Path.IsPathRooted(directory))
            //{
            //    throw new ArgumentException("Can only switch to directories relative to workingDirectory");
            //}

            workspaceManager.SetCurrentDirectory(directory);
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
            }
        }

        /// <inheritdoc/>
        public void CreateFileIfNotExists(string targetFile, string initialContent)
        {
            //TODO : Path should not be relative as well. We are expecting only filename.
            if (Path.IsPathRooted(targetFile))
            {
                throw new ArgumentException($"{targetFile} must be  relative path to working directory .");
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
            throw new FileNotFoundException($"{targetFile} doesn't exist at location {workspaceManager.GetWorkingDirectory()}");
        }

        /// <inheritdoc/>
        public bool HasDocument(string targetDocument)
        {
            return this.workspaceManager.HasDocument(targetDocument);
        }

        /// <inheritdoc/>
        public void AddDocument(string targetDocument, string documentContent)
        {
            this.workspaceManager.AddDocument(targetDocument, documentContent);
        }

        /// <inheritdoc/>
        public void RemoveDocument(string targetDocument)
        {
            if(this.workspaceManager.TryRemoveDocument(targetDocument))
            {
                string documentLocation = Path.Combine(workspaceManager.GetWorkingDirectory(), targetDocument);
                File.Delete(documentLocation);
                return;
               
            }
            throw new Exception($"Failed to remove {targetDocument} from workspace");
        }

        /// <inheritdoc/>
        public void SetContent(string targetDocument, string documentContent)
        {
            _ =  UpdateBufferAsync(new UpdateBufferRequest() { FileName = targetDocument, Buffer = documentContent });           
        }

        /// <inheritdoc/>
        public void SaveDocument(string targetDocument)
        {
            this.workspaceManager.SaveDocument(targetDocument);
        }

        /// <inheritdoc/>
        public bool TryOpenDocument(string targetDocument)
        {
            return this.workspaceManager.TryOpenDocument(targetDocument);
        }

        /// <inheritdoc/>
        public bool TryCloseDocument(string targetDocument)
        {
            return this.workspaceManager.TryCloseDocument(targetDocument);
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
