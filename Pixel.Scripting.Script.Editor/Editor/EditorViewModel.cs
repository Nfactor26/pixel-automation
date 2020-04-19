using Caliburn.Micro;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;

namespace Pixel.Scripting.Script.Editor
{
    public abstract class EditorViewModel : Screen, ICodeEditor, IDisposable
    {       
        protected string targetDocument;
        protected readonly IEditorService editorService;


        protected CodeTextEditor editor;
        public CodeTextEditor Editor => editor;       
       

        public EditorViewModel(IEditorService editorService)
        {
            this.editorService = editorService;          
        }

        public virtual void OpenDocument(string targetDocument, string intialContent)
        {
            this.targetDocument = targetDocument;
            this.editorService.CreateFileIfNotExists(targetDocument, intialContent);
            this.Editor.Text = this.editorService.GetFileContentFromDisk(targetDocument);

            if (!this.editorService.HasDocument(targetDocument))
            {
                this.editorService.AddDocument(targetDocument, this.editor.Text);
            }

            this.editorService.TryOpenDocument(targetDocument);                    
        }

        public void SetContent(string targetDocument, string documentContent)
        {
            this.editorService.SetContent(targetDocument, documentContent);
            this.Editor.Text = documentContent;
        }

        public virtual void CloseDocument(bool save = true)
        {
            if (save)
            {
               this.editorService.SaveDocument(this.targetDocument);
            }
            this.editorService.TryCloseDocument(this.targetDocument);
            Dispose(true);
        }

        public virtual void Activate()
        {
            this.editor.OpenDocument(this.targetDocument);
        }

        public virtual void Deactivate()
        {
            this.editor.CloseDocument();
        }

        protected void SaveDocument()
        {
            this.editorService.SaveDocument(this.targetDocument);
        }

        public async void Save()
        {          
            CloseDocument(true);
            await this.TryCloseAsync(true);
        }

        public async void Cancel()
        {          
            CloseDocument(false);
            await this.TryCloseAsync(false);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            (editor as IDisposable).Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
