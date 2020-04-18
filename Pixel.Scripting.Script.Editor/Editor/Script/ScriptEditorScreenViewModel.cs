using Pixel.Scripting.Editor.Core.Contracts;
using System.Windows;
using System.Windows.Media;

namespace Pixel.Scripting.Script.Editor.Script
{
    public class ScriptEditorScreenViewModel : EditorViewModel , IScriptEditorScreen
    {  
        public ScriptEditorScreenViewModel(IEditorService editorService) : base(editorService)
        {
            this.DisplayName = "Script Editor";
            this.editor = new CodeTextEditor(editorService)
            {
                ShowLineNumbers = true,
                Margin = new Thickness(5),
                FontSize = 23,
                FontFamily = new FontFamily("Consolas")
            };
        }

        public override void OpenDocument(string documentName, string initialContent = "")
        {
            base.OpenDocument(documentName, initialContent);
            Activate();
        }

        protected override void Dispose(bool isDisposing)
        {
            //Clear any custom directory that was set for script editor.
            this.editorService.SwitchToDirectory(null);          
        }
    }
}
