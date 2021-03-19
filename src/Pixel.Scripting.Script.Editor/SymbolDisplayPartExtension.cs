using Pixel.Scripting.Editor.Core.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Pixel.Scripting.Script.Editor
{
    public static class SymbolDisplayPartExtensions
    {
        private const string LeftToRightMarkerPrefix = "\u200e";

        public static string ToVisibleDisplayString(this SymbolDisplayPart part, bool includeLeftToRightMarker)
        {
            var text = part.Text;

            if (includeLeftToRightMarker)
            {
                if (part.Kind == SymbolDisplayPartKind.Punctuation ||
                    part.Kind == SymbolDisplayPartKind.Space ||
                    part.Kind == SymbolDisplayPartKind.LineBreak)
                {
                    text = LeftToRightMarkerPrefix + text;
                }
            }

            return text;
        }
    
        public static Run ToRun(this SymbolDisplayPart text, bool isBold = false)
        {
            var s = text.ToVisibleDisplayString(includeLeftToRightMarker: true);

            var run = new Run(s);

            if (isBold)
            {
                run.FontWeight = FontWeights.Bold;
            }

            switch (text.Kind)
            {
                case SymbolDisplayPartKind.Keyword:
                    run.Foreground = Brushes.Blue;
                    break;
                case SymbolDisplayPartKind.StructName:
                case SymbolDisplayPartKind.EnumName:
                case SymbolDisplayPartKind.TypeParameterName:
                case SymbolDisplayPartKind.ClassName:
                case SymbolDisplayPartKind.DelegateName:
                case SymbolDisplayPartKind.InterfaceName:
                    run.Foreground = Brushes.Teal;
                    break;
            }

            return run;
        }
      
        public static TextBlock ToTextBlock(this IEnumerable<SymbolDisplayPart> text, bool isBold = false)
        {
            var result = new TextBlock { TextWrapping = TextWrapping.Wrap };
            foreach (var part in text)
            {
                result.Inlines.Add(part.ToRun(isBold));
            }

            return result;
        }
    }
}
