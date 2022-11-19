using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Threading.Tasks;

namespace Pixel.Scripting.Script.Editor.MultiEditor
{
    public class DocumentViewModel : Anchorable
    {       
        private readonly IMultiEditor editor;

        public override PaneLocation PreferredLocation => PaneLocation.Left;

        private EditableDocumentViewModel selectedDocument;
        public EditableDocumentViewModel SelectedDocument
        {
            get => this.selectedDocument;
            set
            {
                selectedDocument = value;                
                (this.editor as IConductor).ActivateItemAsync(value);
                NotifyOfPropertyChange(() => CanSave);
            }
        }

        public BindableCollection<EditableDocumentViewModel> Documents { get; private set; } 

        public DocumentViewModel(IMultiEditor editor,  BindableCollection<EditableDocumentViewModel> documents)
        {
            this.DisplayName = "Documents";      
            this.editor = editor;
            this.Documents = documents;          

        }

        public async Task AddDocument()
        {
            var newDocumentViewModel = new NewDocumentViewModel(this.editor, this.Documents);
            var windowManager = IoC.Get<IWindowManager>();
            await windowManager.ShowDialogAsync(newDocumentViewModel);
        }

        public bool CanSave
        {
            get => SelectedDocument?.IsModified ?? false;
        }

        public void Save()
        {
            if(SelectedDocument?.IsModified ?? false)
            {
                this.editor.SaveDocument(SelectedDocument.DocumentName, SelectedDocument.ProjectName);
                SelectedDocument.IsModified = false;
            }
            NotifyOfPropertyChange(() => CanSave);
        }

        public void SaveAll()
        {
            foreach(var document in this.Documents)
            {
                if(document.IsModified)
                {
                    this.editor.SaveDocument(document.DocumentName, document.ProjectName);
                    document.IsModified = false;
                }
            }
        }
   
        public async Task OpenDocument(EditableDocumentViewModel editableDocument)
        {
            if(editableDocument != null)
            {
                //TODO : Opening a document doesn't activate it
                await editor.OpenDocumentAsync(editableDocument.DocumentName, editableDocument.ProjectName);
                this.selectedDocument = editableDocument;
                NotifyOfPropertyChange(() => CanSave);             
            }
        }

        public void DeleteDocument(EditableDocumentViewModel editableDocument)
        {
            if (editableDocument != null && !editableDocument.IsOpen)
            {
                editor.DeleteDocument(editableDocument.DocumentName, editableDocument.ProjectName);
            }          
        }

        /// <summary>
        /// Don't allow closing Document view model toolbar.
        /// </summary>
        /// <returns></returns>
        public override bool CanClose()
        {
            return false;
        }


    }
}
