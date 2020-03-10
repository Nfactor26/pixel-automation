using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.Diagnostics
{
    public class DiagnosticResult
    {
        public string FileName { get; set; }      

        public IEnumerable<DiagnosticLocation> QuickFixes { get; set; }    

        public override string ToString()
        {
            return $"{FileName} -> {string.Join(", ", QuickFixes)}";
        }
    }

    public class DiagnosticResultEx : DiagnosticResult
    {
        public object Id { get; set; } 
    }
}
