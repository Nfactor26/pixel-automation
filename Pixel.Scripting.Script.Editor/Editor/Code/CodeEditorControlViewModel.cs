using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Scripting.Script.Editor.Code
{
    public class CodeEditorControlViewModel : CodeEditorScreenViewModel, ICodeEditorControl
    {
        public CodeEditorControlViewModel(IEditorService editorService) : base(editorService)
        {
          
        }
    }
}
