namespace Pixel.Scripting.Editor.Core.Models.CodeFormat
{

    public class CodeFormatRequest : Request
    {
        /// <summary>
        ///  When true, return just the text changes.
        /// </summary>
        public bool WantsTextChanges { get; set; }
    }
}
