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
    public abstract class MultiEditorViewModel : Conductor<IScreen>.Collection.OneActive, IMultiEditor
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
            //foreach (var document in this.editorService.GetAvailableDocuments())
            //{
            //    Documents.Add(new EditableDocumentViewModel(document));
            //}
            this.Tools.Add(new DocumentViewModel(this, Documents));
        }
      
        public async Task OpenDocumentAsync(string documentName, string ownerProject)
        {
            var targetDocument = Documents.FirstOrDefault(d => d.DocumentName.Equals(documentName))
                ?? throw new ArgumentException($"{documentName} is not available in workspace");
            if(targetDocument.IsOpen)
            {
                return;
            }
          
            if (!this.editorService.TryOpenDocument(documentName, ownerProject))
            {
                throw new Exception($"failed to open document {documentName}");
            }
            
            targetDocument.OpenForEdit(this, this.editorService);
            targetDocument.Editor.Text = this.editorService.GetFileContentFromDisk(documentName);
            targetDocument.Editor.OpenDocument(documentName, ownerProject);
            this.OpenDocuments.Add(targetDocument);           
            await ActivateItemAsync(targetDocument, CancellationToken.None);
        }

        public void CloseDocument(string documentName, string ownerProject, bool save = true)
        {
            var targetDocument = Documents.FirstOrDefault(d => d.DocumentName.Equals(documentName))
                  ?? throw new ArgumentException($"{documentName} is not available in workspace");          
            if (save)
            {
                SaveDocument(documentName, ownerProject);
            }
            this.editorService.TryCloseDocument(documentName, ownerProject);
            this.OpenDocuments.Remove(targetDocument);
            targetDocument.Dispose();
        }

        public bool HasDocument(string documentName, string ownerProject)
        {
            return this.editorService.HasDocument(documentName, ownerProject);
        }

        public async Task AddDocumentAsync(string documentName, string ownerProject, string initialContent, bool openAfterAdd)
        {
            //Todo : MultiEditor should allow adding a new project as well when Editor is configured to allow adding new project
            if(string.IsNullOrEmpty(ownerProject))
            {
                ownerProject = this.Documents.First().ProjectName;
            }
            AddDocument(documentName, ownerProject, initialContent);
            if (openAfterAdd)
            {
                await OpenDocumentAsync(documentName, ownerProject);
            }         

        }

        private void AddDocument(string documentName, string ownerProject, string initialContent)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(documentName)))
            {
                throw new ArgumentException($"{documentName} doesn't have a valid extension. Valid extensions are .cs or .csx");
            }

            if (!Documents.Any(a => a.DocumentName.Equals(documentName)))
            {
                EditableDocumentViewModel editableDocument = new EditableDocumentViewModel(documentName, ownerProject);
                this.Documents.Add(editableDocument);
            }

            if (this.editorService.HasDocument(documentName, ownerProject))
            {
                return;
            }         

            this.editorService.AddDocument(documentName, ownerProject, initialContent);
            this.editorService.CreateFileIfNotExists(documentName, initialContent);
        }

        public void DeleteDocument(string documentName, string ownerProject)
        {
            try
            {
                if (this.editorService.HasDocument(documentName, ownerProject))
                {
                    this.editorService.RemoveDocument(documentName, ownerProject);

                    var documentToRemove = this.Documents.Where(a => a.DocumentName.Equals(documentName)).FirstOrDefault();
                    if (documentToRemove != null)
                    {                    
                        this.Documents.Remove(documentToRemove);
                    }                         
                    logger.Information($"{documentName} was deleted from workspace");
                }                
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public void RenameDocument(string documentName, string newName, string ownerProject)
        {

        }

        public void SaveDocument(string documentName, string ownerProject)
        {
            var targetDocument = Documents.FirstOrDefault(d => d.DocumentName.Equals(documentName))
              ?? throw new ArgumentException($"{documentName} is not available in workspace");
          
            this.editorService.SaveDocument(documentName, ownerProject);
        }      

        public void SetContent(string documentName, string ownerProject, string documentContent)
        {
            var targetDocument = Documents.FirstOrDefault(d => d.DocumentName.Equals(documentName))
               ?? throw new ArgumentException($"{documentName} is not available in workspace");
           
            this.editorService.SetContent(documentName, ownerProject, documentContent);
            targetDocument.Editor.Text = documentContent;
        }

        public override async Task ActivateItemAsync(IScreen item, CancellationToken cancellationToken)
        {  
      
            //TODO : Check why selecting a tool box item causes ActivateItem to be triggered by avalon dock
            if (item == null || item is IToolBox)
            {
                return;
            }
            
            if(item is EditableDocumentViewModel document && document.IsOpen)
            {
                this.ActiveDocument = document; //This is what matters to avalon
                await base.ActivateItemAsync(item, cancellationToken);
            }
        }

        /// <summary>
        /// Close all documents while saving all changes
        /// </summary>
        public async void Save()
        {
            var openDocuments = new List<EditableDocumentViewModel>(this.OpenDocuments);
            foreach (var document in openDocuments)
            {
                CloseDocument(document.DocumentName, document.ProjectName, true);
            }
            await this.TryCloseAsync(true);
        }

       /// <summary>
       /// Close all document without saving any changes when Cancel button is clicked
       /// </summary>
        public async void Cancel()
        {
            this.Close();
            await this.TryCloseAsync(false);
        }

        /// <summary>
        /// Close all documents without saving any change when X button is clicked
        /// </summary>
        public void Close()
        {
            var openDocuments = new List<EditableDocumentViewModel>(this.OpenDocuments);
            foreach (var document in openDocuments)
            {
                CloseDocument(document.DocumentName, document.ProjectName, false);
            }
        }


        protected virtual void Dispose(bool isDisposing)
        { 
            Close();
            foreach(var document in this.Documents)
            {
                document.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);  
        }
    }
}
