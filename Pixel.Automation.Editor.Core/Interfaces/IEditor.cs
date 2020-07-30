using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IEditor : IDisposable
    {
        IObservableCollection<IToolBox> Tools
        {
            get;
        }


        /// <summary>
        /// Save the project state in the current workspace
        /// </summary>
        Task DoSave();      

        /// <summary>
        /// Unload the project
        /// </summary>
        void DoUnload();

        /// <summary>
        /// Manage different versions of project and their deployment
        /// </summary>
        Task Manage();

        /// <summary>
        /// Addd Component to an Entity
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="componentToAdd"></param>
        void AddComponent(Entity parent, IComponent componentToAdd);

        /// <summary>
        /// Remove component from its parent
        /// </summary>
        /// <param name="component"></param>
        void DeleteComponent(IComponent component);

        /// <summary>
        /// Open data model associated with project for editing
        /// </summary>
        Task EditDataModel();
    
    }
}
