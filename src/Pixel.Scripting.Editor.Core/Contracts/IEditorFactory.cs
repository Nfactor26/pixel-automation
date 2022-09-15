﻿using System;
using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface IEditorFactory : IDisposable
    {
        /// <summary>
        /// Initialize the editor factory with initial working directory and a collection of assembly names that should be referenced
        /// by underlying project created by workspace
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="editorReferences"></param>
        /// <param name="imports"></param>
        void Initialize(string workingDirectory, IEnumerable<string> editorReferences, IEnumerable<string> imports);

        /// <summary>
        /// Update the Editor to use a new working directory
        /// </summary>
        /// <param name="workingDirectory">New working directory</param>
        void SwitchWorkingDirectory(string workingDirectory);

        /// <summary>
        /// Add a new document to project
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="projectName"></param>
        /// <param name="documentContent"></param>
        void AddDocument(string documentName, string projectName, string documentContent);

        /// <summary>
        /// Remove an existing document from project
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="projectName"></param>
        void RemoveDocument(string documentName, string projectName);

        /// <summary>
        /// Remove project from solution
        /// </summary>
        /// <param name="projectName"></param>
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

        /// <summary>
        /// Compile a project given it's name
        /// </summary>
        /// <param name="projectName">Name of the project to be compiled</param>
        /// <param name="outputAssemblyName">Desired name of the output assembly</param>
        /// <returns>CompilationResult</returns>
        CompilationResult CompileProject(string projectName, string outputAssemblyName);
    }

    public interface IScriptEditorFactory : IEditorFactory
    {
        void AddProject(string projectName, string[] projectreferences, Type globalsType);

        /// <summary>
        /// Create a script editor screen (shown as a dialog)
        /// </summary>
        /// <returns></returns>
        IScriptEditorScreen CreateScriptEditorScreen();       

        /// <summary>
        /// Create an inline script editor that will automatically remove underlying project on dispose
        /// </summary>
        /// <returns></returns>
        IInlineScriptEditor CreateInlineScriptEditor();

        /// <summary>
        /// Get an existing inline script editor with given identifer or create a new one.
        /// Underlying project will be automatically removed when editor is disposed.
        /// Inline Editors created using this overload are cached. Component views which provide
        /// inline editors are frequently unloaded and created again. When a new view is created,
        /// it tries to create a new inline editor again and we should reuse last created inline 
        /// editor for same component.
        /// </summary>
        /// <param name="cacheKey">Key used to cache the instance</param>
        /// <returns></returns>
        IInlineScriptEditor CreateInlineScriptEditor(string cacheKey);

        /// <summary>
        /// Dispose and remove cahced inline script editor from cache
        /// </summary>
        /// <param name="identifier"></param>
        void RemoveInlineScriptEditor(string identifier);

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
        void Initialize(string workingDirectory, Type globalsType, IEnumerable<string> editorReferences);

        IREPLScriptEditor CreateREPLEditor<T>(T globals);
    }
}
