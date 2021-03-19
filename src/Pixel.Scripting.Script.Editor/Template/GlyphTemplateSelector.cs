using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Script.Editor.Features;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Scripting.Script.Editor
{
    public class GlyphTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            string glyphKey = default;
            if(item is ICompletionDataEx completionDataEx)
            {
                glyphKey = completionDataEx.Glyph.ToString();
            }
            else
            {
                glyphKey = element.Tag?.ToString();
            }               
               
            if(string.IsNullOrEmpty(glyphKey))
            {
                return element.FindResource("MissingGlyph") as DataTemplate;
            }
            DataTemplate glyphTemplate = element.TryFindResource(Enum.Parse(typeof(Glyph),glyphKey)) as DataTemplate;
            return glyphTemplate ?? element.FindResource("MissingGlyph") as DataTemplate;
        }
    }
}
