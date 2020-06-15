using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixel.Automation.Test.Runner
{
    public interface ITestSelector
    {
        bool CanRunCategory(TestCategory testCategory);

        bool CanRunTest(TestCase testCase);
    }

    public class TestSelector : ITestSelector
    {
        private List<string> includedCategories = new List<string>();
        private List<string> excludedCategories = new List<string>();
        private List<string> includedTags = new List<string>();
        private List<string> excludedTags = new List<string>();

        public TestSelector()
        {
           
        }

        public TestSelector WithCategories(string categories)
        {
            includedCategories.AddRange(ParseArgument(categories));
            return this;
        }

        public TestSelector WithExcludedCategories(string categories)
        {
            excludedCategories.AddRange(ParseArgument(categories));
            return this;
        }

        public TestSelector WithTags(string tags)
        {
            includedTags.AddRange(ParseArgument(tags));
            return this;
        }

        public TestSelector WithExcludedTags(string tags)
        {
            excludedTags.AddRange(ParseArgument(tags));
            return this;
        }

        public bool CanRunCategory(TestCategory category)
        {
            bool canRun = !includedCategories.Any() || includedCategories.Contains(category.DisplayName);
            canRun = canRun && excludedCategories.Any() && !excludedCategories.Contains(category.DisplayName);
            return canRun;
        }

        public bool CanRunTest(TestCase testCase)
        {
            bool canRun = !testCase.IsMuted;
            canRun = canRun && (!includedTags.Any() || includedTags.Contains(testCase.DisplayName));
            canRun =  canRun && (excludedTags.Any() && !excludedTags.Contains(testCase.DisplayName));
            return canRun;
        }

        private string[]  ParseArgument(string argument)
        {
            if(string.IsNullOrEmpty(argument))
            {
                return Array.Empty<string>();
            }
            string[] values = argument.Split(new char[] { '|' });
            return values;
        }
    }
}
