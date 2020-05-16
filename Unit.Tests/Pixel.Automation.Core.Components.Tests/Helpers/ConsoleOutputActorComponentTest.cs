using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components.Helpers;
using Pixel.Automation.Core.Interfaces;
using System;
using System.IO;

namespace Pixel.Automation.Core.Components.Tests
{
    public class ConsoleOutputActorComponentTest
    {
        [Test]
        public void AssertThatConsoleOutputActorCanWriteToConsole()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<Argument>()).Returns("Hello World !!");
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            ConsoleOutputActorComponent consoleOutputActorComponent = new ConsoleOutputActorComponent();
            consoleOutputActorComponent.EntityManager = entityManager;

            string consoleOutput = string.Empty;

            var textWriter = Substitute.For<TextWriter>();
            textWriter.When(t => t.WriteLine(Arg.Any<string>())).Do(x => { consoleOutput = x.ArgAt<string>(0); });

            var currentConsoleOut = Console.Out;

            try
            {
                Console.SetOut(textWriter);
                consoleOutputActorComponent.Act();
                Assert.AreEqual("Hello World !!", consoleOutput);

            }
            finally
            {
                Console.SetOut(currentConsoleOut);
            }

        }
    }
}
