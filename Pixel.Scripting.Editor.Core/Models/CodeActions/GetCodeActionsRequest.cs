
using System;
using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.CodeActions
{
    public class GetCodeActionsRequest : Request , ICodeActionRequest
    {
        public Range Selection { get; set; }
    }

    public class Range : IEquatable<Range>
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public bool Contains(int line, int column)
        {
            if (Start.Line > line || End.Line < line)
            {
                return false;
            }

            if (Start.Line == line && Start.Column > column)
            {
                return false;
            }

            if (End.Line == line && End.Column < column)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
            => Equals(obj as Range);

        public bool Equals(Range other)
            => other != null
                && EqualityComparer<Point>.Default.Equals(Start, other.Start)
                && EqualityComparer<Point>.Default.Equals(End, other.End);

        public override int GetHashCode()
        {
            var hashCode = -1676728671;
            hashCode = hashCode * -1521134295 + EqualityComparer<Point>.Default.GetHashCode(Start);
            hashCode = hashCode * -1521134295 + EqualityComparer<Point>.Default.GetHashCode(End);
            return hashCode;
        }

        public override string ToString()
            => $"Start = {{{Start}}}, End = {{{End}}}";

        public static bool operator ==(Range range1, Range range2)
            => EqualityComparer<Range>.Default.Equals(range1, range2);

        public static bool operator !=(Range range1, Range range2)
            => !(range1 == range2);
    }

    public class Point : IEquatable<Point>
    {        
        public int Line { get; set; }

        public int Column { get; set; }

        public override bool Equals(object obj)
            => Equals(obj as Point);

        public bool Equals(Point other)
            => other != null
                && Line == other.Line
                && Column == other.Column;

        public override int GetHashCode()
        {
            var hashCode = -1456208474;
            hashCode = hashCode * -1521134295 + Line.GetHashCode();
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            return hashCode;
        }

        public override string ToString()
            => $"Line = {Line}, Column = {Column}";

        public static bool operator ==(Point point1, Point point2)
            => EqualityComparer<Point>.Default.Equals(point1, point2);

        public static bool operator !=(Point point1, Point point2)
            => !(point1 == point2);
    }
}
