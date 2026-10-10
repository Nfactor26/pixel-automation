namespace Pixel.Automation.Editor.Core.Interfaces
{
    /// <summary>
    /// Factory for creating IEditor.
    /// </summary>
    public interface IEditorFactory
    {
        /// <summary>
        /// Create an instance of IAutomationEditor
        /// </summary>
        /// <returns></returns>
        IAutomationEditor CreateAutomationEditor();       
    }
}
