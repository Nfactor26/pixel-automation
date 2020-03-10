using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Models.Completions;
using System;
using System.Windows.Controls;

namespace Pixel.Scripting.Script.Editor.Features
{
    public interface ICompletionDataEx : ICompletionData
    {
        Glyph Glyph { get;}
    }

    public class CodeCompletionData : ICompletionDataEx
    {
        private Decorator description;
        AutoCompleteResponse completionData;

        public CodeCompletionData(AutoCompleteResponse completionData)
        {
            this.completionData = completionData;
            this.Text = completionData.CompletionText;
            this.Glyph = completionData.Glyph;
        }

        public Glyph Glyph { get; private set; }
       

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get
            {
                return Text;
            }
        }

        public object Description
        {
            get
            {
                if (completionData.SymbolDisplayParts != null && description == null)
                {
                    description = new Decorator();
                    description.Child = completionData.SymbolDisplayParts.ToTextBlock();
                }
                if (description != null)
                    return description;

                return completionData.Description;
            }
        }

        public double Priority
        {
            get
            {
                return 1.0;
            }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {         
            textArea.Document.Replace(completionSegment,this.Text);
        }
    }  
}
