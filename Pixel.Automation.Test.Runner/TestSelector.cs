﻿using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.TestData;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner
{
    public interface ITestSelector
    {
        bool CanRunTest(TestFixture fixture, TestCase testCase);

        Task Initialize(string selector);
    }

    public class TestSelector : ITestSelector
    {

        private readonly IScriptEngine scriptEngine;
        private Func<TestFixture, TestCase, bool> testSelector;

        public TestSelector(IScriptEngineFactory scriptEngineFactory)
        {
            Guard.Argument(scriptEngineFactory).NotNull();
            scriptEngineFactory.WithSearchPaths(Environment.CurrentDirectory, Environment.CurrentDirectory);
            this.scriptEngine = scriptEngineFactory.CreateScriptEngine(Environment.CurrentDirectory);
        }

        public async Task Initialize(string selector)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine($"using {typeof(Priority).Namespace};");
            sb.AppendLine($"using {typeof(TestFixture).Namespace};");
            sb.AppendLine("bool IsMatch(TestFixture fixture, TestCase testCase)");
            sb.AppendLine("{");
            sb.AppendLine($"    return {selector};");
            sb.AppendLine("}");
            sb.AppendLine("return ((Func<TestFixture, TestCase, bool>)IsMatch);");
            File.WriteAllText("Selector.csx", sb.ToString());
            this.testSelector = await scriptEngine.CreateDelegateAsync<Func<TestFixture, TestCase, bool>>("Selector.csx");
        }

        public bool CanRunTest(TestFixture fixture, TestCase testCase)
        {
            bool canRun = !testCase.IsMuted;
            canRun = this.testSelector.Invoke(fixture, testCase);
            return canRun;
        }
    }
}
