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

        IVersionManager CreatePrefabReferenceManager(IProjectFileSystem projectFileSystem);

        IVersionManager CreateControlReferenceManager(IFileSystem projectFileSystem);

        IVersionManager CreateAssemblyReferenceManager(IFileSystem projectFileSystem);
    }
}
