﻿using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Script.Editor.Controls;
using Pixel.Scripting.Script.Editor.Features;
using System;
using System.IO;

namespace Pixel.Scripting.Script.Editor
{
    public partial class CodeTextEditor : TextEditor , IDisposable
    {
        IEditorService editorService;      
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

        partial void InitializeMouseHover();       

        public void OpenDocument(string documentName)
        {
            this.Document.FileName = Path.GetFileName(documentName);
          
            this.textManager = new TextManager(this.editorService, this);
            syntaxHighlighter = new SyntaxHighlightingColorizer(this.Document.FileName, this.editorService, new ClassificationHighlightColors());
            TextArea.TextView.LineTransformers.Insert(0, syntaxHighlighter);

            this.textMarkerService = new TextMarkerService(this);
            TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
            TextArea.TextView.LineTransformers.Add(textMarkerService);

            if(this.editorService.IsFeatureEnabled(EditorFeature.Diagnostics))
            {
                diagnosticsManager = new DiagnosticsManager(this.editorService, this.Document, this.textMarkerService);
            }

            if (this.editorService.IsFeatureEnabled(EditorFeature.CodeActions))
            {
                codeActionsControl = new CodeActionsControl(this, this.editorService);
            }
          

            //var documentChangeObservable = Observable.FromEventPattern<DocumentChangeEventArgs>(this.Document, nameof(Document.Changed));
            //documentChangeObservableSubscription = documentChangeObservable.Throttle(TimeSpan.FromMilliseconds(50)).Subscribe(Observer.Create<EventPattern<DocumentChangeEventArgs>>(d =>
            //{
            //    synchronizationContextScheduler.Schedule(() =>
            //    {
            //        this.editorService.GetWorkspaceManager().ChangeBuffer(documentName, this.Document.Text);
            //    });
            //}));      

            InitializeMouseHover();
        }
     
        public void CloseDocument()
        {
            //TextArea.TextView.LineTransformers.RemoveAt(0);
            //documentChangeObservableSubscription.Dispose();
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


        public void Dispose()
        {
            TextArea.TextView.LineTransformers.RemoveAt(0);          
            MouseHover -= OnMouseHover;
            MouseHoverStopped -= OnHoverStopped;
            TextArea.TextView.BackgroundRenderers.Clear();
            TextArea.TextView.LineTransformers.Clear();          
            diagnosticsManager?.Dispose();
            textManager?.Dispose();
            this.editorService = null;
        }
    }   
}
