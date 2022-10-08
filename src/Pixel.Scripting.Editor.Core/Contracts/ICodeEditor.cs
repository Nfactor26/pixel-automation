using System;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface ICodeEditor : IDisposable
    {
        string GetEditorText();

        void OpenDocument(string documentName, string ownerProject, string initialContent);

        void SetContent(string documentName, string ownerProject, string documentContent);

        void CloseDocument(bool save = true);
    }

    public interface IInlineScriptEditor : ICodeEditor, IDisposable
    {
        void Activate();

        void Deactivate();
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
