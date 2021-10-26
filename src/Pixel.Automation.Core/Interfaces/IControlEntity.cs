using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IControlEntity : IComponent
    {
        /// <summary>
        /// Identifier of the control
        /// </summary>
        string ControlId { get; }

        /// <summary>
        /// Target file where control identification details are saved
        /// </summary>
        string ControlFile { get; set; }

        /// <summary>
        /// Control identification details
        /// </summary>
        IControlIdentity ControlDetails { get; }       

        /// <summary>
        /// FilterMode for finding the control
        /// </summary>
        FilterMode FilterMode { get; set; }

        /// <summary>
        /// When FilterMode is FindAll, Filter argument can be used to provide a script that will filter out all 
        /// unwanted controls but one
        /// </summary>
        Argument Filter { get; set; }       
       
        /// <summary>
        /// When FilterMode is FindAll, index can be specified to pick a specific control amongst multiple matches
        /// </summary>
        Argument Index { get; set; }
       
        /// <summary>
        /// Lookup mode for searching the control
        /// </summary>
        LookupMode LookupMode { get; set; }
      
        /// <summary>
        /// SearchRoot identifies a already looked up control relative to which current control must be looked up.
        /// When SearchRoot is not configured, control lookup starts with the target application window
        /// </summary>
        Argument SearchRoot { get; set; }

        /// <summary>
        /// Get all the found controls
        /// </summary>
        /// <returns></returns>
        IEnumerable<UIControl> GetAllControls();
      
        /// <summary>
        /// Get the found control 
        /// </summary>
        /// <returns></returns>
        UIControl GetControl();      
      
        /// <summary>
        /// Get native found control e.g. IwebElement or AutomationElement instead of UIControl 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetTargetControl<T>();

        /// <summary>
        /// Reload the control details from control file.
        /// This can be used to reload control data on editor whenever control is edited from explorer.
        /// </summary>
        void Reload();
 
    }
}