using Caliburn.Micro;
using Dawn;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Scripting.Script.Editor
{
    public abstract class EditorViewModel : Screen, ICodeEditor, IDisposable
    {
        protected readonly ILogger logger = Log.ForContext<EditorViewModel>();
        protected readonly string Identifier = Guid.NewGuid().ToString();

        protected string targetDocument;
        protected string ownerProject;
        protected readonly IEditorService editorService;

        protected CodeTextEditor editor;
        public CodeTextEditor Editor => editor;

        protected EditorViewModel(IEditorService editorService)
        {
            this.editorService = editorService;          
        }

        public string GetEditorText()
        {
            return this.editor?.Text ?? string.Empty;
        }

        public virtual void OpenDocument(string targetDocument, string ownerProject, string intialContent)
        {
            Guard.Argument(targetDocument).NotNull().NotEmpty();
            Guard.Argument(ownerProject).NotNull().NotEmpty();

            this.targetDocument = targetDocument;
            this.ownerProject = ownerProject;
            this.editorService.CreateFileIfNotExists(targetDocument, intialContent);
            string fileContents = this.editorService.GetFileContentFromDisk(targetDocument);           

            if (!this.editorService.HasDocument(targetDocument, ownerProject))
            {
                this.editorService.AddDocument(targetDocument, ownerProject, fileContents);                
            }
            this.editorService.SetContent(targetDocument, ownerProject, fileContents);
            this.editorService.TryOpenDocument(this.targetDocument, this.ownerProject);
            this.Editor.Text = fileContents;
            this.Editor.OpenDocument(targetDocument, ownerProject);
            this.Editor.ResumeEditorUpdates();
         
        }

        public void SetContent(string targetDocument, string documentContent, string ownerProject)
        {
            Guard.Argument(targetDocument).NotNull().NotEmpty();
            Guard.Argument(ownerProject).NotNull().NotEmpty();
            Guard.Argument(documentContent).NotNull();

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
        }

        protected void SaveDocument()
        {
            this.editorService.SaveDocument(this.targetDocument, this.ownerProject);
        }

        /// <summary>
        /// Save and close document when Save button is clicked.
        /// </summary>
        public async void Save()
        {          
            CloseDocument(true);
            await this.TryCloseAsync(true);
        }

        /// <summary>
        /// Close document when Cancel button is clicked
        /// </summary>
        public async void Cancel()
        {          
            CloseDocument(false);
            await this.TryCloseAsync(false);
        }


        public void SetEditorOptions(EditorOptions editorOptions)
        {           
           this.Editor.ShowLineNumbers = editorOptions.ShowLineNumbers;
           this.Editor.FontSize = editorOptions.FontSize;
           if(!string.IsNullOrEmpty(editorOptions.FontFamily))
            {
                this.Editor.FontFamily = new System.Windows.Media.FontFamily(editorOptions.FontFamily);
            }
        }

        /// <summary>
        /// Called when screen is closed using the X button on top right with close = true otherwise false.
        /// </summary>
        /// <param name="close"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            CloseDocument(false);
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            (editor as IDisposable).Dispose();
            editor = null;
            logger.Debug($"{nameof(EditorViewModel)} with Id : {Identifier} is disposed now.");
        }

        public void Dispose()
        {
            Dispose(true);            
        }
    }
}
