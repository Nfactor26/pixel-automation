using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.Contracts
{
    /// <summary>
    /// Interface implemented by Prefab Builder Wizard
    /// </summary>
    public interface IPrefabBuilder
    {
        /// <summary>
        /// Initialize the wizard with the application and entity details
        /// </summary>
        /// <param name="applicationDescriptionViewModel"></param>
        /// <param name="entity"></param>
        void Initialize(ApplicationDescriptionViewModel applicationDescriptionViewModel, Entity entity);

        /// <summary>
        /// Save the prefab details and return the created PrefabProject instance on completing the wizard
        /// </summary>
        /// <returns></returns>
        Task<PrefabProject> SavePrefabAsync();
    }
}
