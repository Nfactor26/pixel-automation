using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

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

        /// <summary>
        /// Remove a node from control hierarcy.
        /// </summary>
        /// <param name="controlToRemove"></param>
        void RemoveFromControlHierarchy(IControlIdentity controlToRemove);

        /// <summary>
        /// Insert a new node in control hierarchy
        /// </summary>
        /// <param name="controlIdentity"></param>
        void InsertIntoControlHierarchy(IControlIdentity controlIdentity);
    }
}
