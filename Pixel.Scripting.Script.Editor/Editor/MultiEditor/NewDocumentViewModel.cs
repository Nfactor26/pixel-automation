using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Scripting.Script.Editor.MultiEditor
{
    public class NewDocumentViewModel : SmartScreen
    {
        private readonly IMultiEditor editor;
        private readonly IEnumerable<EditableDocumentViewModel> existingDocuments;

        private string documentName;
        public string DocumentName
        {
            get => documentName;
            set
            {
                documentName = value;
                ValidateDocumentName();
                NotifyOfPropertyChange(() => DocumentName);
                NotifyOfPropertyChange(() => CanAddDocument);
            }
        }
   
        public NewDocumentViewModel(IMultiEditor editor, IEnumerable<EditableDocumentViewModel> existingDocuments)
        {
            this.editor = editor;
            this.existingDocuments = existingDocuments;
        }

        public async Task AddDocument()
        {
            await editor.AddDocumentAsync(this.DocumentName, string.Empty, true);         
            await this.TryCloseAsync(true);
        }

        public bool CanAddDocument
        {
            get => !this.HasErrors;
        }

        public async Task Cancel()
        {           
            await this.TryCloseAsync(false);
        }


        private void ValidateDocumentName()
        {
            ClearErrors(nameof(DocumentName));
            if(string.IsNullOrEmpty(this.documentName))
            {
                AddOrAppendErrors(nameof(DocumentName), "Name is required");
            }
            if(string.IsNullOrEmpty(Path.GetExtension(this.documentName)))
            {             
                AddOrAppendErrors(nameof(DocumentName), "Name must have a valid file extension. Valid extension are .cs and .csx");
            }
            if(existingDocuments.Any(a => a.DocumentName.Equals(this.documentName)))
            {
                AddOrAppendErrors(nameof(DocumentName), "Document already exists");
            }
        }
    }
}
