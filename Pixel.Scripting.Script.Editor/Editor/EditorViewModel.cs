using Caliburn.Micro;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;

namespace Pixel.Scripting.Script.Editor
{
    public abstract class EditorViewModel : Screen, ICodeEditor, IDisposable
    {       
        protected string documentName;
        protected readonly IEditorService editorService;


        protected CodeTextEditor editor;
        public CodeTextEditor Editor => editor;       
       

        public EditorViewModel(IEditorService editorService)
        {
            this.editorService = editorService;          
        }

        public virtual void OpenDocument(string documentName, string intialContent)
        {
            this.documentName = Path.GetFileName(documentName);
            this.editorService.CreateFileIfNotExists(this.documentName, intialContent);
            this.Editor.Text = this.editorService.GetFileContentFromDisk(this.documentName);

            if (!this.editorService.HasDocument(this.documentName))
            {
                this.editorService.AddDocument(documentName, this.editor.Text);
            }

            this.editorService.TryOpenDocument(this.documentName);                    
        }

        public void SetContent(string documentName, string documentContent)
        {
            this.editorService.SetContent(documentName, documentContent);
            this.Editor.Text = documentContent;
        }

        public virtual void CloseDocument(bool save = true)
        {
            if (save)
            {
               this.editorService.SaveDocument(this.documentName);
            }
            this.editorService.TryCloseDocument(this.documentName);
            Dispose(true);
        }

        public virtual void Activate()
        {
            this.editor.OpenDocument(documentName);
        }

        public virtual void Deactivate()
        {
            this.editor.CloseDocument();
        }

        protected void SaveDocument()
        {
            this.editorService.SaveDocument(this.documentName);
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
