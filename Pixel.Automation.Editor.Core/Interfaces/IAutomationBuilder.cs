using Pixel.Automation.Core.Models;
using System;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IAutomationBuilder : IEditor, IDisposable
    {
        AutomationProject CurrentProject
        {
            get;
        }

        /// <summary>
        /// Load project from the current workspace
        /// </summary>
        /// <param name="project"></param>
        void DoLoad(AutomationProject project, VersionInfo versionInfo = null);
     
    }
}
