using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Persistence.Services.Client.Interfaces;

public interface IDataManagerFactory
{  
    /// <summary>
    /// Get an instance of <see cref="IProjectAssetsDataManager"/> for a given version of automation project
    /// </summary>
    /// <param name="automationProject"></param>
    /// <param name="versionInfo"></param>
    /// <param name="fileSystem"></param>
    /// <returns></returns>
    IProjectAssetsDataManager CreateProjectAssetsDataManager(AutomationProject automationProject, VersionInfo versionInfo, IProjectFileSystem fileSystem);
}
