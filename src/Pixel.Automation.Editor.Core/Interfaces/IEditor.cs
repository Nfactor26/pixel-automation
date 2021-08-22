using Pixel.Automation.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IEditor : IDisposable
    {
        /// <summary>
        /// Save the project state in the current workspace
        /// </summary>
        Task DoSave();      

        /// <summary>
        /// Manage different versions of project and their deployment
        /// </summary>
        Task Manage();

        /// <summary>
        /// Remove component from its parent
        /// </summary>
        /// <param name="component"></param>
        void DeleteComponent(IComponent component);

        /// <summary>
        /// Open code editor that can be used to add , remove or modify existing data models for the project
        /// </summary>
        Task EditDataModelAsync();    
    }
}
