using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IProjectManager
    {
        IProjectManager WithEntityManager(EntityManager entityManager);

        IFileSystem GetProjectFileSystem();

        Task Save();

        void SaveAs();

        object GetDataModel();

        T Load<T>(string fileName) where T : new();
    }
}
