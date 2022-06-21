using Pixel.Automation.Core.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Control locators are used to locate a control at runtime
    /// </summary>
    public interface IControlLocator
    {      
        /// <summary>
        /// Indicates if the locator can identify a control of specified type.
        /// This is required because an application can be configured to have multiple types of control locator e.g. UIA + Image based
        /// and we must check which locator can process a given type of control and use that.
        /// </summary>
        /// <param name="controlIdentity"></param>
        /// <returns></returns>
        bool CanProcessControlOfType(IControlIdentity controlIdentity);

        /// <summary>
        /// Find the control given the details of control and the search root
        /// </summary>
        /// <param name="controlIdentity"></param>
        /// <param name="searchRoot"></param>
        /// <returns></returns>
        Task<UIControl> FindControlAsync(IControlIdentity controlIdentity, UIControl searchRoot);

        /// <summary>
        /// Find all the controls matching the control details within specified search root
        /// </summary>
        /// <param name="controlIdentity"></param>
        /// <param name="searchRoot"></param>
        /// <returns></returns>
        Task<IEnumerable<UIControl>> FindAllControlsAsync(IControlIdentity controlIdentity, UIControl searchRoot);
    }
   
}
