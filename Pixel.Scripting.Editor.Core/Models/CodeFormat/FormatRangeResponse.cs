using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.CodeFormat
{
    public class FormatRangeResponse
    {
        public IEnumerable<LinePositionSpanTextChange> Changes { get; set; }
    }
}