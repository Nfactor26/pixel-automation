using Pixel.Automation.Core.TestData;
using System;

namespace Pixel.Automation.Editor.Core
{
    public class TestCaseUpdatedEventArgs : EventArgs
    {
        public TestCase ModifiedTestCase { get;}

        public TestCaseUpdatedEventArgs(TestCase testCase)
        {
            this.ModifiedTestCase = testCase;
        }

    }
}
