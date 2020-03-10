using Pixel.Automation.Core.TestData;
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
        /// Run test case
        /// </summary>
        /// <param name="testCaseEntity"></param>
        Task<TestResult> RunTestAsync(Entity testCaseEntity);

    
        /// <summary>
        /// Initialize test data model with underlying data source for given test case and add testCaseEntity to the automation process.
        /// </summary>
        /// <param name="testCase"></param>     
        void OpenTestEntity(TestCase testCase);

        /// <summary>
        /// Remove Test Case Entity from canvas once editing is complete
        /// </summary>
        /// <param name="testCaseEntity"></param>
        void RemoveTestEntity(Entity testCaseEntity);
    }
}
