
namespace Pixel.Scripting.Editor.Core.Models.CodeActions
{
   
    public class RunCodeActionRequest : Request , ICodeActionRequest
    {
        public string Identifier { get; set; }
        public Range Selection { get; set; }
        public bool WantsTextChanges { get; set; }
        public bool ApplyTextChanges { get; set; } = true;
        public bool WantsAllCodeActionOperations { get; set; }
    }
}
