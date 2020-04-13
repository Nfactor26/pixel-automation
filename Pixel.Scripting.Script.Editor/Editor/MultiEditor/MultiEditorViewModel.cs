using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Scripting.Script.Editor.MultiEditor
{
    public class MultiEditorViewModel : Conductor<IScreen>.Collection.OneActive, IMultiEditor
    {
        private readonly ILogger logger = Log.ForContext<MultiEditorViewModel>();
        protected readonly IEditorService editorService;

        public BindableCollection<EditableDocumentViewModel> Documents { get; set; } = new BindableCollection<EditableDocumentViewModel>();

        public BindableCollection<EditableDocumentViewModel> OpenDocuments { get; set; } = new BindableCollection<EditableDocumentViewModel>();

        private EditableDocumentViewModel activeDocument;
        public EditableDocumentViewModel ActiveDocument
        {
            get => activeDocument;
            set
            {
                activeDocument = value;
                NotifyOfPropertyChange(() => ActiveDocument);
            }

        }

        public BindableCollection<IToolBox> Tools { get; set; } = new BindableCollection<IToolBox>();

        public MultiEditorViewModel(IEditorService editorService)
        {
            this.DisplayName = "Editor";
            this.editorService = editorService;       
            foreach (var document in this.editorService.GetAvailableDocuments())
            {
                Documents.Add(new EditableDocumentViewModel(document));
            }
            this.Tools.Add(new DocumentViewModel(this, Documents));
        }
      
        public async Task OpenDocumentAsync(string documentName)
        {
            var targetDocument = Documents.FirstOrDefault(d => d.DocumentName.Equals(documentName))
                ?? throw new ArgumentException($"{documentName} is not available in workspace");
            if(targetDocument.IsOpen)
            {
                return;
            }
          
            if (!this.editorService.TryOpenDocument(documentName))
            {
                throw new Exception($"failed to open document {documentName}");
            }
            
            targetDocument.OpenForEdit(this, this.editorService);
            targetDocument.Editor.Text = this.editorService.GetFileContentFromDisk(documentName);
            targetDocument.Editor.OpenDocument(documentName);
            this.OpenDocuments.Add(targetDocument);           
            await ActivateItemAsync(targetDocument, CancellationToken.None);
        }

        public void CloseDocument(string documentName, bool save = true)
        {
            var targetDocument = Documents.FirstOrDefault(d => d.DocumentName.Equals(documentName))
                  ?? throw new ArgumentException($"{documentName} is not available in workspace");

            if (save)
            {
                SaveDocument(documentName);
            }
            this.editorService.TryCloseDocument(documentName);
            this.OpenDocuments.Remove(targetDocument);
            targetDocument.Dispose();
        }

        public bool HasDocument(string documentName)
        {
            return this.editorService.HasDocument(documentName);
        }

        public async Task AddDocumentAsync(string documentName, string initialContent, bool openAfterAdd)
        {
            if(string.IsNullOrEmpty(Path.GetExtension(documentName)))
            {
                throw new ArgumentException($"{documentName} doesn't have a valid extension. Valid extensions are .cs or .csx");
            }
            if (this.editorService.HasDocument(documentName))
            {
                throw new InvalidOperationException($"Document with name {documentName} already exists");
            }

            EditableDocumentViewModel editableDocument = new EditableDocumentViewModel(documentName);
            this.Documents.Add(editableDocument);

            this.editorService.AddDocument(documentName, initialContent);
            this.editorService.CreateFileIfNotExists(documentName, initialContent);

            if (openAfterAdd)
            {
                await OpenDocumentAsync(documentName);
            }         

        }

        public void DeleteDocument(string documentName)
        {
            try
            {
                this.editorService.RemoveDocument(documentName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public void RenameDocument(string documentName, string newName)
        {

        }

        public void SaveDocument(string documentName)
        {
            var targetDocument = Documents.FirstOrDefault(d => d.DocumentName.Equals(documentName))
              ?? throw new ArgumentException($"{documentName} is not available in workspace");
          
            this.editorService.SaveDocument(documentName);
        }      

        public void SetContent(string documentName, string documentContent)
        {
            var targetDocument = Documents.FirstOrDefault(d => d.DocumentName.Equals(documentName))
               ?? throw new ArgumentException($"{documentName} is not available in workspace");
           
            this.editorService.SetContent(documentName, documentContent);
            targetDocument.Editor.Text = documentContent;
        }

        public override async Task ActivateItemAsync(IScreen item, CancellationToken cancellationToken)
        {  
      
            //TODO : Check why selecting a tool box item causes ActivateItem to be triggered by avalon dock
            if (item == null || item is IToolBox)
            {
                return;
            }
            if(item is EditableDocumentViewModel document)
            {
                this.ActiveDocument = document; //This is what matters to avalon
            }
            await base.ActivateItemAsync(item, cancellationToken);           
        }

        /// <summary>
        /// Close all documents while saving all changes
        /// </summary>
        public async void Save()
        {
            var openDocuments = new List<EditableDocumentViewModel>(this.OpenDocuments);
            foreach (var document in openDocuments)
            {
                CloseDocument(document.DocumentName, true);
            }
            await this.TryCloseAsync(true);
        }

       /// <summary>
       /// Close all document without saving unsaved changes
       /// </summary>
        public async void Cancel()
        {
            var openDocuments = new List<EditableDocumentViewModel>(this.OpenDocuments);
            foreach (var document in openDocuments)
            {
                CloseDocument(document.DocumentName, false);
            }
            await this.TryCloseAsync(true);
        }

        public void Close()
        {
            var openDocuments = new List<EditableDocumentViewModel>(this.OpenDocuments);
            foreach (var document in openDocuments)
            {
                CloseDocument(document.DocumentName, false);
            }
        }     

    }
}
