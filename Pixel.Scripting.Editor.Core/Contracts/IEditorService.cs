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
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface IEditorService
    {
        event EventHandler<WorkspaceChangedEventArgs> WorkspaceChanged;

        IWorkspaceManager GetWorkspaceManager();

        void Initialize(EditorOptions editorOptions, Type globalsType);
               
        void SwitchToDirectory(string directory);

        /// <summary>
        /// Get a collection of all .cs or .csx file in current working directory
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAvailableDocuments();

        bool IsFeatureEnabled(EditorFeature editorFeature);   

        /// <summary>
        /// Create a file on local disk with initial content
        /// </summary>
        /// <param name="targetFile">Relative path of document to working directory</param>
        /// <param name="initialContent">Initial content of the document</param>
        void CreateFileIfNotExists(string targetFile, string initialContent);

        /// <summary>
        /// Get the contents of a file from disk
        /// </summary>
        /// <param name="targetFile">Relative path of document to working directory</param>
        /// <returns></returns>
        string GetFileContentFromDisk(string targetFile);

        /// <summary>
        /// Check if the document is already available in underlying workspace
        /// </summary>
        /// <param name="documentName">Relative path of document to working directory</param>
        /// <returns></returns>
        bool HasDocument(string documentName);

        /// <summary>
        /// Add a new document to underlying workspace with some content
        /// </summary>
        /// <param name="targetDocument">Relative path of the document to working directory</param>
        /// <param name="documentContent">Content of the document</param>
        void AddDocument(string targetDocument, string documentContent);

        /// <summary>
        /// Remove document from underlying workspace
        /// </summary>
        /// <param name="targetDocument">Relative path of the document to working directory</param>
        void RemoveDocument(string targetDocument);

        /// <summary>
        /// Set content of document
        /// </summary>
        /// <param name="targetDocument">Relative path of the document to working directory</param>
        /// <param name="documentContent">New content of the document</param>
        void SetContent(string targetDocument, string documentContent);

        /// <summary>
        /// Save document
        /// </summary>
        /// <param name="targetDocument">Relative path of the document to working directory</param>
        void SaveDocument(string targetDocument);

        /// <summary>
        /// Open a document for edit.
        /// </summary>
        /// <param name="targetDocument">Relative path of the document to working directory</param>
        /// <returns></returns>
        bool TryOpenDocument(string targetDocument);

        /// <summary>
        /// Close document for edit.
        /// </summary>
        /// <param name="targetDocument">Relative path of the document to working directory</param>
        /// <returns></returns>
        bool TryCloseDocument(string targetDocument);
      

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
