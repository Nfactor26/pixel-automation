using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Pixel.Scripting.Script.Editor.MultiEditor
{
    public class EditableDocumentViewModel : Screen , IDisposable
    {      
        private IMultiEditor editor;

        public string DocumentName { get; private set; }

        private bool isOpen;
        public bool IsOpen
        {
            get => isOpen;
            set
            {
                isOpen = value;
                NotifyOfPropertyChange(() => IsOpen);
            }
        }

        public CodeTextEditor Editor { get; private set; }

        public bool IsModified
        {
            get => Editor?.IsModified ?? false;
            set
            {
                if(Editor != null)
                {
                    Editor.IsModified = value;
                }
            }
        }


        private ICommand closeCommand;
        public ICommand CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new RelayCommand(p => CloseDocument(), p => CanClose())); }
        }


        public EditableDocumentViewModel(string fileName)
        {
            this.DocumentName = fileName;
            this.DisplayName = Path.GetFileName(fileName);
        }
       
        public void OpenForEdit(IMultiEditor editor, IEditorService editorService)
        {
            this.editor = editor;          
            this.Editor = new CodeTextEditor(editorService)
            {
                ShowLineNumbers = true,
                Margin = new Thickness(5),
                FontSize = 23,
                FontFamily = new FontFamily("Consolas")
            };
            this.IsOpen = true;
            NotifyOfPropertyChange(() => Editor);
            NotifyOfPropertyChange(() => IsOpen);
        }

        public bool CanClose()
        {
            return true;
        }

        public async void CloseDocument()
        {
            bool saveChanges = false;
            if(this.IsModified)
            {
               var result =  MessageBox.Show("Would you like to save changes?", "Save Changes", MessageBoxButton.OKCancel, MessageBoxImage.Question);
               saveChanges = result.Equals(MessageBoxResult.OK);
            }
            this.editor.CloseDocument(this.DocumentName, saveChanges);
            await base.OnDeactivateAsync(true, CancellationToken.None);
        }

        public void Dispose()
        {
            Dispose(true);           
        }

        protected virtual void Dispose(bool isDisposing)
        {         
            this.editor = null;
            this.Editor?.Dispose();
            this.IsOpen = false;
        }       
    }
}
