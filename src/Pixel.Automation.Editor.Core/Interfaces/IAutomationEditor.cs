using Pixel.Automation.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IAutomationEditor : IEditor, IDisposable
    {
        /// <summary>
        /// Project loaded in to Automation Editor
        /// </summary>
        AutomationProject CurrentProject { get; }

        /// <summary>
        /// Load project from the current workspace
        /// </summary>
        /// <param name="project"></param>
        Task DoLoad(AutomationProject project, VersionInfo versionInfo = null);

        /// <summary>
        /// Manage prefab references for the project.
        /// </summary>
        /// <returns></returns>

        Task ManagePrefabReferencesAsync();       

    }
}
