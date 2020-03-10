using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Editor.Core.Models.CodeActions;

namespace Pixel.Scripting.Editor.Services.CSharp.Helpers
{
    public static class TextExtensions
    {
        /// <summary>
        /// Converts a zero-based position in a <see cref="SourceText"/> to an OmniSharp <see cref="Point"/>.
        /// </summary>
        public static Point GetPointFromPosition(this SourceText text, int position)
        {
            var line = text.Lines.GetLineFromPosition(position);

            return new Point
            {
                Line = line.LineNumber,
                Column = position - line.Start
            };
        }

        /// <summary>
        /// Converts a line number and offset to a zero-based position within a <see cref="SourceText"/>.
        /// </summary>
        public static int GetPositionFromLineAndOffset(this SourceText text, int lineNumber, int offset)
            => text.Lines[lineNumber].Start + offset;

        /// <summary>
        /// Converts an OmniSharp <see cref="Point"/> to a zero-based position within a <see cref="SourceText"/>.
        /// </summary>
        public static int GetPositionFromPoint(this SourceText text, Point point)
            => text.GetPositionFromLineAndOffset(point.Line, point.Column);

        /// <summary>
        /// Converts a <see cref="TextSpan"/> in a <see cref="SourceText"/> to an OmniSharp <see cref="Range"/>.
        /// </summary>
        public static Range GetRangeFromSpan(this SourceText text, TextSpan span)
            => new Range
            {
                Start = text.GetPointFromPosition(span.Start),
                End = text.GetPointFromPosition(span.End)
            };

        /// <summary>
        /// Converts an OmniSharp <see cref="Range"/> to a <see cref="TextSpan"/> within a <see cref="SourceText"/>.
        /// </summary>
        public static TextSpan GetSpanFromRange(this SourceText text, Range range)
            => TextSpan.FromBounds(
                start: text.GetPositionFromPoint(range.Start),
                end: text.GetPositionFromPoint(range.End));
    }
}
