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
        void DoSave();      

        /// <summary>
        /// Unload the project
        /// </summary>
        void DoUnload();

        /// <summary>
        /// Save project with a new version
        /// </summary>
        void CreateSnapShot();

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
