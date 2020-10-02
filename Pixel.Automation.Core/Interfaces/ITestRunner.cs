using Pixel.Automation.Core.TestData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    public interface ITestRunner
    {
        /// <summary>
        /// Indicates if test case can be executed.
        /// For ex, this should be false if Environment SetUp is not done yet.
        /// </summary>
        bool CanRunTests { get; }

        /// <summary>
        /// Perform environment setup e.g. launching required applications
        /// </summary>
        /// <returns></returns>
        Task SetUpEnvironment();


        /// <summary>
        /// Perform environment teardown e.g. close applications 
        /// </summary>
        /// <returns></returns>
        Task TearDownEnvironment();

        /// <summary>
        /// Perform one time setup for test fixture
        /// </summary>
        /// <returns></returns>
        Task OneTimeSetUp(TestFixture fixture);


        /// <summary>
        /// Perform one time tear down for test fixture
        /// </summary>
        /// <returns></returns>
        Task OneTimeTearDown(TestFixture fixture);


        /// <summary>
        ///  Run test case asynchronously for each of test data in data source
        /// </summary>
        /// <param name="fixture">TestFixture to which TestCase belongs</param>
        /// <param name="testCase">TestCase to run</param>
        /// <returns>TestResult for each execution</returns>
        IAsyncEnumerable<TestResult> RunTestAsync(TestFixture fixture, TestCase testCase);

        /// <summary>
        /// Try to open test fixture for editing
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        Task<bool> TryOpenTestFixture(TestFixture fixture);


        /// <summary>
        /// Try to close an open test fixture
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        Task<bool> TryCloseTestFixture(TestFixture fixture);

        /// <summary>
        /// Try to open test case for editing. Parent fixture is also opened if not already open.
        /// </summary>
        /// <param name="fixture">TestFixture to which TestCase belongs</param>     
        /// <param name="testCase">TestCase to open</param>     
        Task<bool> TryOpenTestCase(TestFixture fixture, TestCase testCase);

        /// <summary>
        /// Try to close an open test caes
        /// </summary>
        /// <param name="testCaseEntity"></param>
        Task<bool> TryCloseTestCase(TestFixture fixture, TestCase testCase);
    }
}
