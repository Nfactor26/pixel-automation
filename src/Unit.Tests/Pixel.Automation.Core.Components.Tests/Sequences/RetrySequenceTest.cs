using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class RetrySequenceTest
    {
        /// <summary>
        /// Validate that retry sequence attempts configured number of tries and processes retry block before
        /// each retry attempt.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ValidateThatRetrySequenceAttemptsConfiguredNumberOfRetries()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<int>(Arg.Any<Argument>()).Returns(5); //we want to retry upto 5 times.
            argumentProcessor.GetValue<double>(Arg.Any<Argument>()).Returns(1); // we want to retry every 1 second
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            
            var retrySequence = new RetrySequence();
            retrySequence.EntityManager = entityManager;
            retrySequence.ResolveDependencies();


            //create a actor for execute block that throws exception twice and works fine 3rd time
            var executeActor = Substitute.For<ActorComponent>();
            executeActor.When(x => x.Act()).
                Do(Callback.First(
                    x => { throw new Exception(); }).Then(
                    x => { throw new Exception(); }).Then(
                    x => { }));
            var executeBlock = retrySequence.GetComponentsByName("Execute").Single() as Entity;
            executeBlock.AddComponent(executeActor);
            
            var retryActor = Substitute.For<ActorComponent>();
            var retryBlock = retrySequence.GetComponentsByName("Retry").Single() as Entity;
            retryBlock.AddComponent(retryActor);

            await retrySequence.BeginProcess();

            executeActor.Received(3).Act();
            retryActor.Received(2).Act();
        }
    }
}
