using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Pixel.Scripting.Script.Editor.Features
{
    public class CachedHighlightedLine
    {
        public IDocumentLine DocumentLine { get; }

        public int LineNumber
        {
            get => DocumentLine.LineNumber;
        }

        public int Length { get; set; }

        public int Offset { get; set; }

        public int EndOffset { get; set; }

        public ITextSourceVersion DocumentVersion { get; set; }

        public HighlightedLine HighlightedLine { get; }

        internal CachedHighlightedLine(IDocumentLine documentLine, HighlightedLine highlightedLine)
        {
            this.DocumentLine = documentLine;
            this.HighlightedLine = highlightedLine;
            Offset = documentLine.Offset;
            EndOffset = documentLine.EndOffset;
            DocumentVersion = highlightedLine.Document.Version;
            Length = documentLine.Length;
        }
    }
}
