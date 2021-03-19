using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.CodeFormat
{
    public class CodeFormatResponse
    {
        public string Buffer { get; set; }

        public IEnumerable<LinePositionSpanTextChange> Changes { get; set; }
    }
}
