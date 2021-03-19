using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pixel.Scripting.Script.Editor.Script
{
    public class InlineScriptEditorViewModel : IInlineScriptEditor, INotifyPropertyChanged
    {
        private string targetDocument;
        private string ownerProject;
        private readonly IEditorService editorService;

        public CodeTextEditor Editor { get; private set; }

        public InlineScriptEditorViewModel(IEditorService editorService)
        {
            this.editorService = editorService;
            this.Editor = new CodeTextEditor(editorService)
            {
                ShowLineNumbers = false,
                Margin = new Thickness(2),
                FontSize = 14,
                FontFamily = new FontFamily("Consolas"),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                WordWrap = true
            };
            this.Editor.LostFocus += OnLostFocus;
            this.Editor.GotFocus += OnFocus;
            this.editorService.WorkspaceChanged += OnWorkspaceChanged;
        }

        public void SetEditorOptions(EditorOptions editorOptions)
        {
            this.Editor.ShowLineNumbers = editorOptions.ShowLineNumbers;
            this.Editor.FontSize = editorOptions.FontSize;
            if (!string.IsNullOrEmpty(editorOptions.FontFamily))
            {
                this.Editor.FontFamily = new System.Windows.Media.FontFamily(editorOptions.FontFamily);
            }
        }


        private void OnWorkspaceChanged(object sender, WorkspaceChangedEventArgs e)
        {
            this.Editor.LostFocus -= OnLostFocus;
            this.Editor.GotFocus -= OnFocus;
            (this.Editor as IDisposable)?.Dispose();

            this.Editor = new CodeTextEditor(editorService)
            {
                ShowLineNumbers = false,
                Margin = new Thickness(2),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                WordWrap = true
            };
            OpenDocument(this.targetDocument, this.ownerProject, string.Empty);
            this.Editor.LostFocus += OnLostFocus;
            this.Editor.GotFocus += OnFocus;
            OnPropertyChanged(nameof(Editor));          
        }

        private void OnFocus(object sender, RoutedEventArgs e)
        {
            Activate();         
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            Deactivate();
        }

        public void OpenDocument(string targetDocument, string ownerProject, string initialContent)
        {          
            this.targetDocument = targetDocument;
            this.ownerProject = ownerProject;
            this.editorService.CreateFileIfNotExists(targetDocument, initialContent);
            string fileContents = this.editorService.GetFileContentFromDisk(targetDocument);
            if (!this.editorService.HasDocument(this.targetDocument, this.ownerProject))
            {
                this.editorService.AddDocument(this.targetDocument, this.ownerProject, fileContents);
                this.editorService.SetContent(targetDocument, ownerProject, fileContents);
            }          
            this.Editor.OpenDocument(targetDocument, ownerProject);
            this.Editor.Text = fileContents;
        }


        public void SetContent(string documentName, string ownerProject, string documentContent)
        {
            this.editorService.SetContent(targetDocument, ownerProject, documentContent);
            this.Editor.Text = documentContent;
        }
     
        public void CloseDocument(bool save = true)
        {
            if(this.targetDocument != null)
            {
                if (save)
                {
                    this.editorService.SaveDocument(this.targetDocument, this.ownerProject);
                }
                this.editorService.TryCloseDocument(this.targetDocument, this.ownerProject);
            }           
        }

        public void Activate()
        {
            this.editorService.TryOpenDocument(this.targetDocument, this.ownerProject);
            this.Editor.ResumeEditorUpdates();
        }

        public void Deactivate()
        {
            CloseDocument(true);
            this.Editor.SuspendEditorUpdates();
        }

        protected virtual void Dispose(bool isDisposing)
        {
            CloseDocument(false);
            this.editorService.WorkspaceChanged -= OnWorkspaceChanged;
            (this.Editor as IDisposable)?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);           
        }

        #region INotifyPropertyChanged

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName]string name = "")
        {
            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion INotifyPropertyChanged
    }
}
