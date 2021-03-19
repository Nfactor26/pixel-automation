namespace Pixel.Scripting.Editor.Core.Models.CodeFormat
{
    public class FormatRangeRequest : Request
    {     
        public int EndLine { get; set; }
      
        public int EndColumn { get; set; }
    }
}
