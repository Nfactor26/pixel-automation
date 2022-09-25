using Dawn;
using Pixel.Automation.Core;
using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pixel.Scripting.Script.Editor.Script
{
    public class InlineScriptEditorViewModel : NotifyPropertyChanged, IInlineScriptEditor 
    {
        private readonly ILogger logger = Log.ForContext<InlineScriptEditorViewModel>();
        private readonly string Identifier = Guid.NewGuid().ToString();

        private string targetDocument;
        private string ownerProject;
        private readonly IEditorService editorService;

        public CodeTextEditor Editor { get; private set; }

        public InlineScriptEditorViewModel(IEditorService editorService)
        {
            Guard.Argument(editorService).NotNull();

            this.editorService = editorService;
            this.Editor = new CodeTextEditor(editorService)
            {
                EnableDiagnostics = false,
                EnableCodeActions = false,
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

            logger.Debug($"Created a new instance of {nameof(InlineScriptEditorViewModel)} with Id : {Identifier}");
        }

        public string GetEditorText()
        {
            return this.Editor?.Text ?? string.Empty;
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
            logger.Debug($"Process workspace changed event for {nameof(InlineScriptEditorViewModel)} with Id : {Identifier}");

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
            Debug.Assert(!string.IsNullOrEmpty(targetDocument));
            Debug.Assert(!string.IsNullOrEmpty(ownerProject));
            if(!string.IsNullOrEmpty(targetDocument) && !string.IsNullOrEmpty(ownerProject))
            {
                OpenDocument(this.targetDocument, this.ownerProject, string.Empty);
            }
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
            Guard.Argument(targetDocument).NotNull().NotEmpty();
            Guard.Argument(ownerProject).NotNull().NotEmpty();

            this.targetDocument = targetDocument;
            this.ownerProject = ownerProject;
            this.editorService.CreateFileIfNotExists(targetDocument, initialContent);
            string fileContents = this.editorService.GetFileContentFromDisk(targetDocument);
            if (!this.editorService.HasDocument(this.targetDocument, this.ownerProject))
            {
                this.editorService.AddDocument(this.targetDocument, this.ownerProject, fileContents);                
            }
            this.editorService.SetContent(targetDocument, ownerProject, fileContents);
            this.Editor.Text = fileContents;
            this.Editor.OpenDocument(targetDocument, ownerProject);           
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
            this.editorService.TryRemoveProject(this.ownerProject);
            this.editorService.WorkspaceChanged -= OnWorkspaceChanged;
            this.Editor.LostFocus -= OnLostFocus;
            this.Editor.GotFocus -= OnFocus;
            this.Editor.Dispose();
            this.Editor = null;                   
            logger.Debug($"{nameof(InlineScriptEditorViewModel)} with Id : {Identifier} is disposed now.");
        }

        public void Dispose()
        {
            Dispose(true);           
        }      
    }
}
