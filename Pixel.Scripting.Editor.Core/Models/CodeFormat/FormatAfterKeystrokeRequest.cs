namespace Pixel.Scripting.Editor.Core.Models.CodeFormat
{
    public class FormatAfterKeystrokeRequest : Request
    {
        public string Character { get; set; }

        public char Char { get { return string.IsNullOrEmpty(Character) ? (char)0 : Character[0]; } }
    }
}
