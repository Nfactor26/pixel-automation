using Pixel.Scripting.Editor.Core.Models.FileOperations;
using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.CodeActions
{
    public class RunCodeActionResponse
    {
        public IEnumerable<FileOperationResponse> Changes { get; set; }
    }
}
