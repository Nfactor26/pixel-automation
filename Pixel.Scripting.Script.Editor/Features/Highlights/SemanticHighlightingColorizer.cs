using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Pixel.Scripting.Editor.Core.Contracts;
using System;

namespace Pixel.Scripting.Script.Editor.Features
{
    public class SyntaxHighlightingColorizer : HighlightingColorizer
    {
        private readonly string documentName;
        private readonly string projectName;
        private readonly IEditorService editorService;
        private readonly IClassificationHighlightColors classificationHighlightColors;

        public SyntaxHighlightingColorizer(string documentName, string projectName, IEditorService editorService, IClassificationHighlightColors classificationHighlightColors)
        {
            this.documentName = documentName;
            this.projectName = projectName;
            this.editorService = editorService;
            this.classificationHighlightColors = classificationHighlightColors;
        }

        protected override IHighlighter CreateHighlighter(TextView textView, TextDocument document)
        {
            var highlighter =  new SemanticHighlighter(this.documentName, this.projectName, textView, document, this.editorService, this.classificationHighlightColors);
            document.LineTrackers.Add(highlighter);
            return highlighter;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            try
            {
                base.ColorizeLine(line);
            }
            catch (Exception ex)
            {
               //TODO : We have exception here where sometimes Highlight being applied is outside the bounds of document line.
            }
        }

    }
}
