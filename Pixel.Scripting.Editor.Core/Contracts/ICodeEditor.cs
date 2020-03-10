namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface ICodeEditor
    {
        void OpenDocument(string documentName, string initialContent);

        void SetContent(string documentName, string documentContent);

        void CloseDocument(bool save = true);

        void Activate();

        void Deactivate();
    }


    public interface IInlineScriptEditor : ICodeEditor
    {

    }   

    public interface IScriptEditorScreen : ICodeEditor
    {

    }

    public interface ICodeEditorControl : ICodeEditor
    {    
        
    }

    public interface ICodeEditorScreen : ICodeEditor
    {

    }

    public interface IREPLScriptEditor
    {
        void SetGlobals(object globals);
    }
}
