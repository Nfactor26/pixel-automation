using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.FileOperations
{
    public class ModifiedFileResponse : FileOperationResponse
    {
        public ModifiedFileResponse(string fileName)
            : base(fileName, FileModificationType.Modified)
        {
        }

        public string Buffer { get; set; }
        public IEnumerable<LinePositionSpanTextChange> Changes { get; set; }
    }
}
