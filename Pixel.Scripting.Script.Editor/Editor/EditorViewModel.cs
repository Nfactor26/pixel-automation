using Caliburn.Micro;
using Pixel.Scripting.Editor.Core.Contracts;
using System;

namespace Pixel.Scripting.Script.Editor
{
    public abstract class EditorViewModel : Screen, ICodeEditor, IDisposable
    {       
        protected string targetDocument;
        protected string ownerProject;
        protected readonly IEditorService editorService;


        protected CodeTextEditor editor;
        public CodeTextEditor Editor => editor;       
       

        public EditorViewModel(IEditorService editorService)
        {
            this.editorService = editorService;          
        }

        public virtual void OpenDocument(string targetDocument, string ownerProject, string intialContent)
        {
            this.targetDocument = targetDocument;
            this.ownerProject = ownerProject;
            this.editorService.CreateFileIfNotExists(targetDocument, intialContent);
            string fileContents = this.editorService.GetFileContentFromDisk(targetDocument);           

            if (!this.editorService.HasDocument(targetDocument, ownerProject))
            {
                this.editorService.AddDocument(targetDocument, ownerProject, fileContents);
                this.editorService.SetContent(targetDocument, ownerProject, fileContents);

            }
            this.editorService.TryOpenDocument(this.targetDocument, this.ownerProject);
            this.Editor.OpenDocument(targetDocument, ownerProject);
            this.Editor.Text = this.editorService.GetFileContentFromDisk(targetDocument);
        }

        public void SetContent(string targetDocument, string documentContent, string ownerProject)
        {
            this.editorService.SetContent(targetDocument, ownerProject, documentContent);
            this.Editor.Text = documentContent;
        }

        public virtual void CloseDocument(bool save = true)
        {
            if (save)
            {
               this.editorService.SaveDocument(this.targetDocument, this.ownerProject);
            }
            this.editorService.TryCloseDocument(this.targetDocument, this.ownerProject);
            Dispose(true);
        }

        public virtual void Activate()
        {
            this.editor.OpenDocument(this.targetDocument, this.ownerProject);
        }

        public virtual void Deactivate()
        {
            this.editor.CloseDocument();
        }

        protected void SaveDocument()
        {
            this.editorService.SaveDocument(this.targetDocument, this.ownerProject);
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
            editor = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
