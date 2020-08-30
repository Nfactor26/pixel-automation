﻿using System;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface ICodeEditor : IDisposable
    {
        void OpenDocument(string documentName, string ownerProject, string initialContent);

        void SetContent(string documentName, string ownerProject, string documentContent);

        void CloseDocument(bool save = true);

        void Activate();

        void Deactivate();
    }


    public interface IInlineScriptEditor : ICodeEditor, IDisposable
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
