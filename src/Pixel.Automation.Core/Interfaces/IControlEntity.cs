using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IControlEntity : IComponent
    {
        /// <summary>
        /// Identifier of the owner application
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// Identifier of the control
        /// </summary>
        string ControlId { get; }

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
        /// Get all the located controls based on configured control identity
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<UIControl>> GetAllControls();
      
        /// <summary>
        /// Get the control located based on configured control identity
        /// </summary>
        /// <returns></returns>
        Task<UIControl> GetControl();             

        /// <summary>
        /// Reload the control details from control file.
        /// This can be used to reload control data on editor whenever control is edited from explorer.
        /// </summary>
        void Reload();
 
    }
}