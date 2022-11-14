using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Reference.Manager.Contracts;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IVersionManager : IScreen
    {
    }

    public interface IVersionManagerFactory
    {
        IVersionManager CreateProjectVersionManager(AutomationProject automationProject);

        IVersionManager CreatePrefabVersionManager(PrefabProject prefabProject);

        IVersionManager CreatePrefabReferenceManager(IReferenceManager referenceManager);

        IVersionManager CreateControlReferenceManager(IReferenceManager referenceManager);

        IVersionManager CreateAssemblyReferenceManager(IFileSystem projectFileSystem, IReferenceManager referenceManager);
    }
}
