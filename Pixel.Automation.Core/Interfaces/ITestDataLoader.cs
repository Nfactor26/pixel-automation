using Pixel.Automation.Core.TestData;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface ITestDataLoader
    {
        IEnumerable<object> GetTestCaseData(TestCase testCase);
    }
}
