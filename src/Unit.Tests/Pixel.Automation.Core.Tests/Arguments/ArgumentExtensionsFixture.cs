using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Argument = Pixel.Automation.Core.Arguments.Argument;

namespace Pixel.Automation.Core.Tests.Arguments
{
    class ArgumentExtensionsFixture
    {
        [Test]
        public async Task ValidateThatValueCanBeRetrievedFromAnArgumentUsingGetValueExtensionMethod()
        {
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<int>(Arg.Any<Argument>()).Returns(10);
            var argument = new OutArgument<int>();

            var result = await argument.GetValue(argumentProcessor);

            Assert.AreEqual(10, result);
        }

        [Test]
        public async Task ValidatethatArgumentValueCanBeSetUsingSetValueExtensionMethod()
        {
            var argument = new InArgument<int>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.When(x => x.SetValueAsync<int>(Arg.Any<Argument>(), Arg.Any<int>())).Do(x => argument.DefaultValue = x.ArgAt<int>(1));

            await argument.SetValue(argumentProcessor, 10);

            Assert.AreEqual(10, argument.DefaultValue);

        }

        [Test]
        public void ValidateThatInitialScriptCanBeGeneratedForInArgument()
        {
            var argument = new InArgument<int>();
            var expected = "using System;\r\nInt32 GetValue()\r\n{\r\n    return default;\r\n}\r\nreturn ((Func<Int32>)GetValue);";
            var result = argument.GenerateInitialScript();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ValidateThatInitialScriptCanBeGeneratedForOutArgument()
        {
            var argument = new OutArgument<int>();
            var expected = "using System;\r\nvoid SetValue(Int32 argumentValue)\r\n{\r\n}\r\nreturn ((Action<Int32>)SetValue);";
            var result = argument.GenerateInitialScript();
            Assert.AreEqual(expected, result);            
        }

        public record FuncArgumentTestDataRecord(Argument argument, string expectedGeneratedScript);
        static IEnumerable<FuncArgumentTestDataRecord> GetFuncArgumentTypes()
        {
            yield return new FuncArgumentTestDataRecord(new FuncArgument<Func<IComponent, bool>>(),
                "#r \"Pixel.Automation.Core.dll\"" +
                "\r\nusing System;" +
                "\r\nusing Pixel.Automation.Core.Interfaces;" +
                "\r\nBoolean DoAction(IComponent a)" +
                "\r\n{" +
                "\r\n" +
                "\r\n}" +
                "\r\n" +
                "\r\nreturn ((Func<IComponent, Boolean>)DoAction);");
            yield return new FuncArgumentTestDataRecord(new FuncArgument<Func<IComponent, List<string>, ValueTask>>(),
                $"#r \"Pixel.Automation.Core.dll\"" +
                "\r\nusing System;" +              
                "\r\nusing Pixel.Automation.Core.Interfaces;" +
                "\r\nusing System.Collections.Generic;" +
                "\r\nusing System.Threading.Tasks;" +
                "\r\nasync ValueTask DoActionAsync(IComponent a, List<String> b)" +
                "\r\n{" +
                "\r\n" +
                "\r\n}" +
                "\r\n" +
                "\r\nreturn ((Func<IComponent, List<String>, ValueTask>)DoActionAsync);");
            yield break;
        }

        [TestCaseSource(nameof(GetFuncArgumentTypes))]
        public void ValidateThatInitialScriptCanBeGeneratedForFuncArguments(FuncArgumentTestDataRecord testDataRecord)
        {
            var result = testDataRecord.argument.GenerateInitialScript();
            Assert.AreEqual(testDataRecord.expectedGeneratedScript, result);
        }
    }
}
