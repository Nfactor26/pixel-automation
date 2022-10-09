using System.Diagnostics.SymbolStore;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public class EditorOptions
    {
        public static EditorOptions DefaultOptions { get; set; } = new EditorOptions();

        public bool EnableDiagnostics { get; set; } = false;

        public bool EnableCodeActions { get; set; } = false;

        public int FontSize { get; set; } = 13;

        public string FontFamily { get; set; } = "Consolas";

        public bool ShowLineNumbers { get; set; } = false;

        public int Thickness { get; set; } = 2;
     
    }
}
