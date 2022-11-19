namespace Pixel.Scripting.Editor.Core.Models.Editors;

/// <summary>
/// DocumentState provides the state of document that might have been edited in the code/script editor.
/// After the editor is closed, DocumentState for each document belonging to the editor will be retrieved.
/// If the user decied to save the changes, the state will be applied.
/// </summary>
public class DocumentState
{
    /// <summary>
    /// Path of the document file
    /// </summary>
    public string TargetDocument { get; set; }

    /// <summary>
    /// Indicates if this is a new documented added in the editor
    /// </summary>
    public bool IsNewDocument { get; set; }

    /// <summary>
    /// Indiciates if the file was modified in the editor
    /// </summary>
    public bool IsModified { get; set; }

    /// <summary>
    /// Indiciates if the file was deleted in the editor
    /// </summary>
    public bool IsDeleted { get; set; }
}
