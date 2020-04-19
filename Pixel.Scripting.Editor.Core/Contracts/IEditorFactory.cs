using System;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface IEditorFactory
    {        
    }

    public interface ICodeEditorFactory : IEditorFactory
    {        
        /// <summary>
        /// Initialize the editor factory with initial working directory and a collection of assembly names that should be referenced
        /// by underlying project created by workspace
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="editorReferences"></param>
        void Initialize(string workingDirectory, string[] editorReferences);

        /// <summary>
        /// Create a  standalone code editor screen with ok and cancel buttons.
        /// </summary>
        /// <returns></returns>
        ICodeEditorScreen CreateCodeEditor();

        /// <summary>
        /// Create a code editor control that can be embedded in some other control
        /// </summary>
        /// <returns></returns>
        ICodeEditorControl CreateCodeEditorControl();

        /// <summary>
        /// Create a code editor window that supports opening and editing multiple documents
        /// </summary>
        /// <returns></returns>
        IMultiEditor CreateMultiCodeEditorScreen();

        /// <summary>
        /// Create a code editor control  that supports opening and editing multiple documents
        /// </summary>
        /// <returns></returns>
        IMultiEditor CreateMultiCodeEditorControl();

        /// <summary>
        /// Get the underlying workspace manager associated with any of the code editors created by this factory.
        /// This can be used to compile project , add documents, remove documents , etc to the workspace.
        /// </summary>
        /// <returns></returns>
        ICodeWorkspaceManager GetWorkspaceManager();
    }

    public interface IScriptEditorFactory : IEditorFactory
    {

        /// <summary>
        /// Initialize ScriptEditorFactory with a working directory , globals type available to script and any assembly references
        /// that should be available for scripts to use
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="globalsType"></param>
        /// <param name="editorReferences"></param>
        void Initialize(string workingDirectory, Type globalsType, string[] editorReferences);

        /// <summary>
        /// Create a script editor
        /// </summary>
        /// <returns></returns>
        IScriptEditorScreen CreateScriptEditor();

        /// <summary>
        /// Create a script editor with a specified working directory.
        /// This script editor will read and write files from specified working directory.
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <returns></returns>
        IScriptEditorScreen CreateScriptEditor(string workingDirectory);

        /// <summary>
        /// Create an inline script editor
        /// </summary>
        /// <returns></returns>
        IInlineScriptEditor CreateInlineScriptEditor();

        /// <summary>
        /// Get the underlying workspace manager associated with any of the script editors created by this factory.
        /// This can be used to  add documents, remove documents , etc to the workspace without actually opening editor.
        /// <returns></returns>
        IScriptWorkspaceManager GetWorkspaceManager();
     
    }

    public interface IREPLEditorFactory : IEditorFactory
    {
        void Initialize(string workingDirectory, Type globalsType, string[] editorReferences);

        IREPLScriptEditor CreateREPLEditor<T>(T globals);
    }
}
