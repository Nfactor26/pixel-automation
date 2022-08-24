using Pixel.Automation.Editor.Core.ViewModels;
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
        Task ManageProjectVersionAsync();

        /// <summary>
        /// Remove a component from view
        /// </summary>
        /// <param name="componentViewModel"></param>
        void DeleteComponent(ComponentViewModel componentViewModel);

        /// <summary>
        /// Open code editor that can be used to add , remove or modify existing data models for the project
        /// </summary>
        Task EditDataModelAsync();

        /// <summary>
        /// Manage control references for the project.
        /// </summary>
        /// <returns></returns>
        Task ManageControlReferencesAsync();
    }
}
