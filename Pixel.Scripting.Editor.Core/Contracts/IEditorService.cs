using Pixel.Scripting.Editor.Core.Models;
using Pixel.Scripting.Editor.Core.Models.CodeActions;
using Pixel.Scripting.Editor.Core.Models.CodeFormat;
using Pixel.Scripting.Editor.Core.Models.Completions;
using Pixel.Scripting.Editor.Core.Models.Diagnostics;
using Pixel.Scripting.Editor.Core.Models.Highlights;
using Pixel.Scripting.Editor.Core.Models.Signatures;
using Pixel.Scripting.Editor.Core.Models.TypeLookup;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface IEditorService
    {
        event EventHandler<WorkspaceChangedEventArgs> WorkspaceChanged;

        IWorkspaceManager GetWorkspaceManager();

        void Initialize(EditorOptions editorOptions, Type globalsType);
               
        void SwitchToDirectory(string directory);

        bool IsFeatureEnabled(EditorFeature editorFeature);   

        void CreateFileIfNotExists(string documentName, string initialContent);

        string GetFileContentFromDisk(string documentName);

        bool HasDocument(string documentName);

        void AddDocument(string documentLocation, string documentContent);

        void SetContent(string documentName, string documentContent);

        void SaveDocument(string documentName);

        bool TryOpenDocument(string documentName);

        bool TryCloseDocument(string documentName);
      

        /// <summary>
        /// Replace the entire content of document
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task UpdateBufferAsync(UpdateBufferRequest request);

        /// <summary>
        /// Apply the changes on the document
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task ChangeBufferAsync(ChangeBufferRequest request);   

        Task<HighlightResponse> GetHighlightsAsync(HighlightRequest request);

        Task<TypeLookupResponse> GetTypeDescriptionAsync(TypeLookupRequest request);

        Task<IEnumerable<AutoCompleteResponse>> GetCompletionsAsync(AutoCompleteRequest request);

        Task<SignatureHelpResponse> GetSignaturesAsync(SignatureHelpRequest request);

        IObservable<DiagnosticResultEx> DiagnosticsUpdated { get; }

        Task<CodeFormatResponse> GetFormattedCodeAsync(CodeFormatRequest formatDocumentRequest);

        Task<FormatRangeResponse> GetFormattedRangeAsync(FormatRangeRequest formatRangeRequest);

        Task<FormatRangeResponse> GetFormattedCodeAfterKeystrokeAsync(FormatAfterKeystrokeRequest formatAfterKeystrokeRequest);

        Task<FormatRangeResponse> GetCommentedTextAsync(FormatRangeRequest formatRangeRequest);

        Task<FormatRangeResponse> GetUnCommentedTextAsync(FormatRangeRequest formatRangeRequest);

        Task<GetCodeActionsResponse> GetCodeActionsAsync(GetCodeActionsRequest getCodeActionsRequest);

        Task<RunCodeActionResponse> RunCodeActionAsync(RunCodeActionRequest runCodeActionsRequest);
    }
}
