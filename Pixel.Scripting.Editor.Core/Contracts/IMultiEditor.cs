using System;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    /// <summary>
    /// Interface for code editor that supports editing multiple documents at once
    /// i.e. documents can be opened in tabs.
    /// </summary>
    public interface IMultiEditor : IDisposable
    {
        /// <summary>
        /// Open a document for edit
        /// </summary>
        /// <param name="documentName"></param>
        Task OpenDocumentAsync(string documentName, string ownerProject);

        /// <summary>
        /// Close document
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="save"></param>
        void CloseDocument(string documentName, string ownerProject, bool save);


        /// <summary>
        /// Check if workspace has a document with specified name
        /// </summary>
        /// <param name="documentName"></param>
        /// <returns></returns>
        bool HasDocument(string documentName, string ownerProject);

        /// <summary>
        /// Add a new document to workspace
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="ownerProject"></param>
        /// <param name="initialContent"></param>
        /// <param name="openAfterAdd"></param>
        Task AddDocumentAsync(string documentName, string ownerProject, string initialContent, bool openAfterAdd);

        /// <summary>
        /// Auto generate the default content for document and add to workspace
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="ownerProject"></param>
        /// <param name="openAfterAdd"></param>
        /// <returns></returns>
        Task AddNewDocumentAsync(string documentName, string ownerProject, bool openAfterAdd);

        /// <summary>
        /// Delete document
        /// </summary>
        /// <param name="documentName"></param>
        void DeleteDocument(string documentName, string ownerProject);

        /// <summary>
        /// Rename document
        /// </summary>
        /// <param name="currentName"></param>
        /// <param name="newName"></param>
        void RenameDocument(string documentName, string newName, string ownerProject);

        /// <summary>
        /// Save document contents
        /// </summary>
        /// <param name="documentName"></param>
        void SaveDocument(string documentName, string ownerProject);
    }
}
