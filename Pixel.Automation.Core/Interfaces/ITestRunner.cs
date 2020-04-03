using Pixel.Automation.Core.TestData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    public interface ITestRunner
    {
        bool CanRunTests { get; }

        /// <summary>
        /// Process One Time Set Up Components
        /// </summary>
        void SetUp();

        /// <summary>
        /// Process One Time Tear Down components
        /// </summary>
        void TearDown();

        /// <summary>
        ///  Run test case asynchronously for each of test data in data source
        /// </summary>
        /// <param name="testCase"></param>
        /// <returns>TestResult for each execution</returns>
        IAsyncEnumerable<TestResult> RunTestAsync(TestCase testCase);

    
        /// <summary>
        /// Initialize Test Case with and add it to TestFixture
        /// </summary>
        /// <param name="testCase"></param>     
        Task<bool> TryOpenTestCase(TestCase testCase);

        /// <summary>
        /// Remove test entity from TestFixture
        /// </summary>
        /// <param name="testCaseEntity"></param>
        Task CloseTestCase(TestCase testCase);
    }
}
