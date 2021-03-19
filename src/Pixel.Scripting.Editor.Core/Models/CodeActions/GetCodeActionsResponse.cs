using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.CodeActions
{
    public class GetCodeActionsResponse
    {
        public IEnumerable<EditorCodeAction> CodeActions { get; set; }
    }
}
