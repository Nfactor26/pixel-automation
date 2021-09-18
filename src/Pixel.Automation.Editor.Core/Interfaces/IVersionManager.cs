using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IVersionManager : IScreen
    {
    }

    public interface IVersionManagerFactory
    {
        IVersionManager CreateProjectVersionManager(AutomationProject automationProject);

        IVersionManager CreatePrefabVersionManager(PrefabProject prefabProject);

        IVersionManager CreatePrefabReferenceManager(IFileSystem projectFileSystem);
    }
}
