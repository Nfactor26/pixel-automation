using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Editor.Core.Models;
using Pixel.Scripting.Editor.Core.Models.Completions;
using Pixel.Scripting.Editor.Core.Models.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace Pixel.Scripting.Script.Editor.Features
{
    class TextManager : IDisposable
    {
        private readonly IEditorService editorService;
        private readonly CodeTextEditor textEditor;  
        private OverloadInsightWindow insightWindow;
        private CompletionWindow completionWindow;
        private Brush completionBackGround;
      
        public TextManager(IEditorService editorService, CodeTextEditor textEditor)
        {
            this.editorService = editorService;
            this.textEditor = textEditor;         
            this.textEditor.Document.Changed += OnDocumentChanged;
            this.textEditor.TextArea.TextEntering += OnTextEntering;
            this.textEditor.TextArea.TextEntered += OnTextEntered;
            this.completionBackGround = CreateDefaultCompletionBackground();
        }

        private void OnDocumentChanged(object sender, DocumentChangeEventArgs e)
        {

            this.editorService.UpdateBufferAsync(new UpdateBufferRequest() { FileName = this.textEditor.Document.FileName, Buffer = this.textEditor.Document.Text });

            //TextLocation editStart;
            //TextLocation editEnd;
            //string newText;
            //if (e.InsertionLength > 0)
            //{
            //    editStart = this.textEditor.Document.GetLocation(e.Offset);
            //    editEnd = this.textEditor.Document.GetLocation(e.Offset + e.InsertionLength);
            //    newText = e.InsertedText.Text;
            //}
            //else if (e.RemovalLength > 0)
            //{
            //    editStart = this.textEditor.Document.GetLocation(e.Offset);
            //    editEnd = this.textEditor.Document.GetLocation(e.Offset + e.RemovalLength);
            //    newText = string.Empty;
            //}
            //else
            //{
            //    return;
            //}

            //ChangeBufferRequest changeBufferRequest = new ChangeBufferRequest()
            //{
            //    FileName = this.textEditor.Document.FileName,
            //    StartColumn = editStart.Column - 1,
            //    StartLine = editStart.Line - 1,
            //    EndColumn = editEnd.Column - 1,
            //    EndLine = editEnd.Line - 1,
            //    NewText = newText
            //};
            //this.editorService.ChangeBufferAsync(changeBufferRequest);         

        }

        private async void OnTextEntered(object sender, TextCompositionEventArgs args)
        {
            if (args.Text[0] == '(')
            {

                if (insightWindow == null)
                {
                    var currentLocation = this.textEditor.Document.GetLocation(this.textEditor.CaretOffset);
                    var signatureOverloads = await editorService.GetSignaturesAsync(new SignatureHelpRequest()
                    {
                        FileName = this.textEditor.FileName,
                        ProjectName = this.textEditor.ProjectName,
                        Line = currentLocation.Line - 1,
                        Column = currentLocation.Column - 2
                    });
                    if (signatureOverloads?.Signatures.Any() ?? false)
                    {
                        insightWindow = new OverloadInsightWindow(this.textEditor.TextArea)
                        {
                            Provider = new MethodOverloadProvider(signatureOverloads),
                            Background = completionBackGround
                        };
                        insightWindow.Closed += delegate
                        {
                            insightWindow = null;
                        };
                        insightWindow.Show();
                    }
                    return;
                }
            }

            if (completionWindow == null)
            {
                char triggerChar = args.Text[0];
                var wordToComplete = char.IsLetterOrDigit(args.Text[0]) ? args.Text[0].ToString() : "";
                var currentLocation = this.textEditor.Document.GetLocation(this.textEditor.CaretOffset);
                var completions = await this.editorService.GetCompletionsAsync(new AutoCompleteRequest()
                {
                    FileName = this.textEditor.FileName,
                    ProjectName = this.textEditor.ProjectName,
                    Line = currentLocation.Line - 1,
                    Column = currentLocation.Column - 1,
                    TriggerCharacter = triggerChar,
                    WantKind = true,
                    WantMethodHeader = true,
                    WantReturnType = true,
                    WordToComplete = wordToComplete.ToString(),
                    WantDocumentationForEveryCompletionResult = true
                });
                if (!completions.Any())
                    return;
                if (!completionWindow?.IsVisible ?? true)
                {
                    insightWindow?.Close();
                    completionWindow = new CompletionWindow(this.textEditor.TextArea) { MinWidth = 300 };
                    if (char.IsLetterOrDigit(triggerChar))
                    {
                        completionWindow.StartOffset -= 1;
                    }
                    IList<ICompletionData> completionDatas = completionWindow.CompletionList.CompletionData;
                    completionDatas.Clear();
                    foreach (var item in completions)
                    {
                        ICompletionData completionData = new CodeCompletionData(item);
                        completionDatas.Add(completionData);
                    }
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                    };
                    completionWindow.Show();
                }
            }
        }

        private void OnTextEntering(object sender, TextCompositionEventArgs args)
        {
            if (args.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(args.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(args);
                }
            }
            //Do not set e.Handled = true.
            // We still want to insert the character that was typed.
        }

        private SolidColorBrush CreateDefaultCompletionBackground()
        {
            var defaultCompletionBackground = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            defaultCompletionBackground.Freeze();
            return defaultCompletionBackground;
        }

        public void Dispose()
        {
            this.textEditor.Document.Changed -= OnDocumentChanged;
            this.textEditor.TextArea.TextEntering -= OnTextEntering;
            this.textEditor.TextArea.TextEntered -= OnTextEntered;
            this.completionBackGround = null;       
        }
    }
}
