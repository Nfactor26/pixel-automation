using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface ITestExplorer
    {
        /// <summary>
        /// Add a new test fixture to the automation project
        /// </summary>
        /// <returns></returns>
        Task AddTestFixtureAsync();

        /// <summary>
        /// Open a TestFixture for edit given it's fixtureId
        /// </summary>
        /// <param name="fixtureId"></param>
        /// <returns></returns>
        Task OpenTestFixtureAsync(string fixtureId);

        /// <summary>
        /// Close an open TestFixture given it's fixtureId 
        /// </summary>
        /// <param name="fixtureId"></param>
        /// <returns></returns>
        Task CloseTestFixtureAsync(string fixtureId);

        /// <summary>
        /// Open a TestCase for edit given it's testCaseId
        /// </summary>
        /// <param name="testCaseId"></param>
        /// <returns></returns>
        Task OpenTestCaseAsync(string testCaseId);

        /// <summary>
        /// Close an open TestCase given it's testCaseId
        /// </summary>
        /// <param name="testCaseId"></param>
        /// <returns></returns>
        Task CloseTestCaseAsync(string testCaseId);

        /// <summary>
        /// Execute the OneTimeSetUpEntity component from automation process
        /// </summary>
        /// <returns></returns>
        Task SetUpEnvironmentAsync();

        /// <summary>
        /// Execute the OneTimeTearDownEntity component from automation process
        /// </summary>
        /// <returns></returns>
        Task TearDownEnvironmentAsync();

        /// <summary>
        /// Save all the open test cases locally on disk
        /// Changes are pushed to persistent storage when project is saved.
        /// </summary>
        void SaveAll();
    }
}
