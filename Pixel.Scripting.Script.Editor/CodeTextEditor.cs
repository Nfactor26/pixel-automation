using Dawn;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Script.Editor.Controls;
using Pixel.Scripting.Script.Editor.Features;
using System;
using System.IO;
using System.Linq;

namespace Pixel.Scripting.Script.Editor
{
    public partial class CodeTextEditor : TextEditor , IDisposable
    {
        /// <summary>
        /// Name of the file currently open in editor
        /// </summary>
        public string FileName
        {
            get => this.Document.FileName;
            private set
            {
                this.Document.FileName = Path.GetFileName(value);
            }
        }

        /// <summary>
        /// Name of the project to which File belongs
        /// </summary>
        public string ProjectName
        {
            get;
            private set;
        }


        private IEditorService editorService;      
        private CodeActionsControl codeActionsControl;
        private IVisualLineTransformer syntaxHighlighter;
        private TextMarkerService textMarkerService;
        private DiagnosticsManager diagnosticsManager;
        private TextManager textManager;
      
        public CodeTextEditor(IEditorService editorService)
        {
            this.editorService = editorService;
            Options = new TextEditorOptions
            {
                ConvertTabsToSpaces = true,
                AllowScrollBelowDocument = true,
                IndentationSize = 4,
                EnableEmailHyperlinks = false,
            };
            ShowLineNumbers = true;          

        }

        partial void SubscribeToMouseEvents();

        partial void UnSubscribeFromMouseEvents();

        public void OpenDocument(string documentName, string projectName)
        {
            Guard.Argument(documentName).NotNull().NotEmpty();
            Guard.Argument(projectName).NotNull().NotEmpty();

            if(!string.IsNullOrEmpty(this.FileName) && !string.IsNullOrEmpty(this.ProjectName))
            {
                throw new InvalidOperationException($"File : {this.FileName} is already open in Editor");
            }

            this.FileName = documentName;
            this.ProjectName = projectName;
            
            this.textManager = new TextManager(this.editorService, this);
            syntaxHighlighter = new SyntaxHighlightingColorizer(this.FileName, this.ProjectName, this.editorService, new ClassificationHighlightColors());
            TextArea.TextView.LineTransformers.Insert(0, syntaxHighlighter);

            this.textMarkerService = new TextMarkerService(this);
            TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
            TextArea.TextView.LineTransformers.Add(textMarkerService);

            if(this.editorService.IsFeatureEnabled(EditorFeature.Diagnostics))
            {
                this.diagnosticsManager = new DiagnosticsManager(this.editorService, this, this.Document, this.textMarkerService);
            }

            if (this.editorService.IsFeatureEnabled(EditorFeature.CodeActions))
            {
                this.codeActionsControl = new CodeActionsControl(this, this.editorService);
            }
          
            SubscribeToMouseEvents();

            //Set it to false once since setting the text while opening sets IsModified to true.
            this.IsModified = false;
        }
     
        /// <summary>
        /// Close the document
        /// </summary>
        public void CloseDocument()
        {
            Dispose(true);
        }         
      
        /// <summary>
        /// Suspend any document updates e.g. reacting to mouse overs
        /// </summary>
        public void SuspendEditorUpdates()
        {
            UnSubscribeFromMouseEvents();
            var semanticHighlighter = this.Document.LineTrackers.FirstOrDefault(a => a is SemanticHighlighter) as SemanticHighlighter;
            semanticHighlighter?.SuspendHighlight();
        }

        /// <summary>
        /// Resume processing document updates e.g. reacting to mouse overs
        /// </summary>
        public void ResumeEditorUpdates()
        {
            SubscribeToMouseEvents();
            var semanticHighlighter = this.Document.LineTrackers.FirstOrDefault(a => a is SemanticHighlighter) as SemanticHighlighter;
            semanticHighlighter?.ResumeHighlight();
        }


        /// <summary>
        /// Gets the document used for code completion, can be overridden to provide a custom document
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>The document of this text editor.</returns>
        protected virtual IDocument GetCompletionDocument(out int offset)
        {
            offset = CaretOffset;
            return Document;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            UnSubscribeFromMouseEvents();
            TextArea.TextView.BackgroundRenderers.Clear();
            TextArea.TextView.LineTransformers.Clear();
            foreach (var tracker in TextArea.Document.LineTrackers)
            {
                if (tracker is IDisposable dt)
                {
                    dt.Dispose();
                }
            }
            diagnosticsManager?.Dispose();
            codeActionsControl?.Dispose();
            textManager?.Dispose();
            this.editorService = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }   
}
