using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.Diagnostics
{
    public class DiagnosticMessage
    {
        public IEnumerable<DiagnosticResult> Results { get; set; }
    }
}