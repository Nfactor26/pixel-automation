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
        /// Remove a component from view
        /// </summary>
        /// <param name="componentViewModel"></param>
        Task DeleteComponent(ComponentViewModel componentViewModel);

        /// <summary>
        /// Open code editor that can be used to add , remove or modify existing data models for the project
        /// </summary>
        Task EditDataModelAsync();

        /// <summary>
        /// Open script editor that can be used to modify the initialization script for the automation project.
        /// </summary>
        /// <returns></returns>
        Task EditScriptAsync();

        /// <summary>
        /// Manage control references for the project.
        /// </summary>
        /// <returns></returns>
        Task ManageControlReferencesAsync();


        /// <summary>
        /// Managed assembly references and imports for code editor, script editor and script engine
        /// </summary>
        /// <returns></returns>
        Task ManageAssemblyReferencesAsync();
    }
}
