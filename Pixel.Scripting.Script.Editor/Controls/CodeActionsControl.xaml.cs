using ICSharpCode.AvalonEdit.Editing;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Editor.Core.Models.CodeActions;
using Pixel.Scripting.Editor.Core.Models.FileOperations;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Pixel.Scripting.Script.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CodeActionsControl.xaml
    /// </summary>
    public partial class CodeActionsControl : Popup , IDisposable
    {
        private readonly CodeTextEditor textEditor;
        private readonly IEditorService editorService;
        public ObservableCollection<EditorCodeAction> AvailableCodeActions { get; private set; } = new ObservableCollection<EditorCodeAction>();

        Caret lastCaretPos = default;

        public CodeActionsControl(CodeTextEditor textEditor, IEditorService editorService)
        {
            InitializeComponent();

            this.textEditor = textEditor;
            this.editorService = editorService;
            this.textEditor.TextArea.Caret.PositionChanged += OnCaretPositionChanged;
            this.textEditor.TextArea.TextView.ScrollOffsetChanged += OnScrollChanged;
            this.textEditor.TextArea.TextView.LostFocus += OnFocusLost;
            this.PlacementTarget = textEditor.TextArea;
            this.Placement = PlacementMode.Relative;
            this.HorizontalOffset = 25.0f;

        }

        private void OnFocusLost(object sender, RoutedEventArgs e)
        {
            this.IsOpen = false;
        }

        private void OnScrollChanged(object sender, EventArgs e)
        {
            var caretPos = lastCaretPos;
            if (caretPos != null)
            {           
                UpdateVerticalOffset(caretPos);
            }
        }

        private async void OnCaretPositionChanged(object sender, EventArgs e)
        {
            Caret caret = sender as Caret;
            var codeActionResponse = await this.editorService.GetCodeActionsAsync(new GetCodeActionsRequest()
            {
                FileName = this.textEditor.FileName,
                ProjectName = this.textEditor.ProjectName,
                Line = caret.Line - 1,
                Column = caret.Column - 1
            });

            if (codeActionResponse == null)
                return;

            lastCaretPos = caret;
            this.CodeActionsList.Visibility = Visibility.Collapsed;
            if (!codeActionResponse.CodeActions.Any())
            {
                this.IsOpen = false;
                return;
            }
                     
            this.AvailableCodeActions.Clear();
            foreach (var codeAction in codeActionResponse.CodeActions)
            {
                this.AvailableCodeActions.Add(codeAction);
            }

            this.IsOpen = true;
            UpdateVerticalOffset(caret);
        }

        private void UpdateVerticalOffset(Caret caret)
        {
            double currentLineStart = this.textEditor.TextArea.TextView.GetVisualTopByDocumentLine(caret.Line);
            double lineHeight = this.textEditor.TextArea.TextView.GetVisualLine(caret.Line)?.Height ?? 15;

            this.CodeActionButton.Height = lineHeight;
            this.VerticalOffset = currentLineStart - this.textEditor.TextArea.TextView.ScrollOffset.Y;
        }

        private async void ApplyCodeAction(object sender, RoutedEventArgs e)
        {
            var codeActionToApply = (sender as Button).DataContext as EditorCodeAction;
            RunCodeActionRequest runCodeActionRequest = new RunCodeActionRequest()
            {
                FileName = this.textEditor.FileName,
                ProjectName = this.textEditor.ProjectName,
                Line = lastCaretPos.Line - 1,
                Column = lastCaretPos.Column - 1,
                WantsTextChanges = true,
                ApplyTextChanges = false,
                Identifier = codeActionToApply.Identifier
            };
            try
            {
                var runCodeActionResponse = await this.editorService.RunCodeActionAsync(runCodeActionRequest);
                if (runCodeActionResponse == null)
                {
                    return;
                }

                this.textEditor.BeginChange();
                foreach (var change in runCodeActionResponse.Changes)
                {
                    if (change is ModifiedFileResponse modifyResponse)
                    {
                        foreach (var modifyChange in modifyResponse.Changes)
                        {
                            int startOffset = this.textEditor.Document.GetOffset(modifyChange.StartLine + 1, modifyChange.StartColumn + 1);
                            int endOffset = this.textEditor.Document.GetOffset(modifyChange.EndLine + 1, modifyChange.EndColumn + 1);
                            this.textEditor.Document.Replace(startOffset, (endOffset - startOffset), modifyChange.NewText);
                        }
                    }
                }
                this.textEditor.EndChange();
            }
            catch (Exception ex)
            {
                //While applying some code actions such as add reference to missing dll for scripting project, we have error.
                //TODO : Check why does it happen. Is it because of nature or project where some code actions are not applicable?
                Debug.Assert(false, ex.Message); 
            }               
           
        }

        private void ToggleCodeActionsVisibility(object sender, RoutedEventArgs e)
        {
            this.CodeActionsList.Visibility = this.CodeActionsList.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        }

        public void Dispose()
        {
            this.textEditor.TextArea.Caret.PositionChanged -= OnCaretPositionChanged;
            this.textEditor.TextArea.TextView.ScrollOffsetChanged -= OnScrollChanged;
            this.textEditor.TextArea.TextView.LostFocus -= OnFocusLost;
        }
    }
}
