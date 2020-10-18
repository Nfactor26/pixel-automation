using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface ITestRepositoryManager
    {
        void Initialize();

        Task OpenTestFixtureAsync(string fixtureId);

        Task CloseTestFixtureAsync(string fixtureId);

        Task OpenTestCaseAsync(string testCaseId);

        Task CloseTestCaseAsync(string testCaseId);

        void SaveAll();
    }
}
