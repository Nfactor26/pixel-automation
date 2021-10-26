using Pixel.Automation.Core.Controls;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    /// <summary>
    /// IControlEditor 
    /// </summary>
    public interface IControlEditor
    {
        /// <summary>
        /// Initialize the control editor with a given control
        /// </summary>
        /// <param name="targetControl">targetControl is the control that is being opened for edit</param>
        void Initialize(ControlDescription targetControl);
      
    }
}
