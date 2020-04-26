using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IProjectManager
    {
        IProjectManager WithEntityManager(EntityManager entityManager);

        IFileSystem GetProjectFileSystem();

        void Save();

        void SaveAs();

        object GetDataModel();

        T Load<T>(string fileName) where T : new();
    }
}
