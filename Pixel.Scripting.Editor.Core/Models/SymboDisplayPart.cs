namespace Pixel.Scripting.Editor.Core.Models
{
    public class SymbolDisplayPart
    {
        public string Text { get; }

        public SymbolDisplayPartKind Kind { get; }

        public SymbolDisplayPart(string text, SymbolDisplayPartKind kind)
        {
            this.Text = text;
            this.Kind = kind;
        }
    }
}
