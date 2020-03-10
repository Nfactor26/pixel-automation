using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Designer.ViewModels
{
    public interface IComponentBox
    {
        BindableCollection<ComponentToolBoxItem> Components
        {
            get;           
        }

        /// <summary>
        /// Create <see cref="ComponentToolBox"/> items for provided components for specified owner.
        /// These items won't be shown unless ShowCustomComponets is called for the specified owner.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="components"></param>
        void AddCustomComponents(string owner, List<Type> components);

        /// <summary>
        /// Remove all the custom components for a given owner
        /// </summary>
        /// <param name="owner"></param>
        void RemoveCustomComponents(string owner);

        /// <summary>
        /// Hide custom componets for a given owner from ui
        /// </summary>
        /// <param name="owner"></param>
        void HideCustomComponents(string owner);

        /// <summary>
        /// Show custom components for a given owner on ui
        /// </summary>
        /// <param name="owner"></param>
        void ShowCustomComponents(string owner);

    }
}
