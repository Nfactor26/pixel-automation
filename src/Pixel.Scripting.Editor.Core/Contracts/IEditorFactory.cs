﻿using System;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface IEditorFactory
    {
        /// <summary>
        /// Initialize the editor factory with initial working directory and a collection of assembly names that should be referenced
        /// by underlying project created by workspace
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="editorReferences"></param>
        void Initialize(string workingDirectory, string[] editorReferences);

        void AddDocument(string documentName, string projectName, string documentContent);

        void RemoveDocument(string documentName, string projectName);

        void RemoveProject(string projectName);
    }

    public interface ICodeEditorFactory : IEditorFactory
    {       
        void AddProject(string projectName, string defaultNameSpace, string[] projectreferences);

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

        CompilationResult CompileProject(string projectName, string outputAssemblyName);
    }

    public interface IScriptEditorFactory : IEditorFactory
    {
        void AddProject(string projectName, string[] projectreferences, Type globalsType);

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
        /// Add additional locations from which #r assemblies can be resolved from.
        /// </summary>
        /// <param name="searchPaths"></param>
        void AddSearchPaths(params string[] searchPaths);

        /// <summary>
        /// Remove specified locations from which #r assemblies can be resolved from.
        /// </summary>
        /// <param name="searchPaths"></param>
        void RemoveSearchPaths(params string[] searchPaths);

    }

    public interface IREPLEditorFactory : IEditorFactory
    {
        void Initialize(string workingDirectory, Type globalsType, string[] editorReferences);

        IREPLScriptEditor CreateREPLEditor<T>(T globals);
    }
}