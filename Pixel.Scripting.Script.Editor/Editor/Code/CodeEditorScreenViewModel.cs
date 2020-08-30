using Pixel.Scripting.Editor.Core.Contracts;
using System.Windows;
using System.Windows.Media;

namespace Pixel.Scripting.Script.Editor.Code
{
    public class CodeEditorScreenViewModel : EditorViewModel, ICodeEditorScreen
    {
        public CodeEditorScreenViewModel(IEditorService editorService) : base(editorService)
        {
            this.DisplayName = "Code Editor";
            this.editor = new CodeTextEditor(editorService)
            {
                ShowLineNumbers = true,
                Margin = new Thickness(5),
                FontSize = 23,
                FontFamily = new FontFamily("Consolas")
            };
        }    

        public override void OpenDocument(string documentName, string ownerProject, string initialContent)
        {
            base.OpenDocument(documentName, ownerProject, initialContent ?? string.Empty);
            Activate();
        }
    }
}
