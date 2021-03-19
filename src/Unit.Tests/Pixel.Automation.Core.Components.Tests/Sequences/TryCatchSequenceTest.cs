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
    public class TryCatchSequenceTest
    {

        /// <summary>
        /// Validate that catch block is not executed if there is no exception in try block , however, finally block is executed
        /// </summary>
        [Test]
        public async Task ValidateThatCatchBlockIsNotExecutedWhenNoErrorInTryBlockAndFinallyBlockIsAlwaysExecuted()
        {
            var entityManager = Substitute.For<IEntityManager>();          

            var tryCatchSequence = new TryCatchSequence();
            tryCatchSequence.EntityManager = entityManager;
            tryCatchSequence.ResolveDependencies();

            var tryActor = Substitute.For<ActorComponent>();           
            var tryBlock = tryCatchSequence.GetComponentsByName("Try").Single() as Entity;
            tryBlock.AddComponent(tryActor);

            var catchActor = Substitute.For<ActorComponent>();
            var catchBlock = tryCatchSequence.GetComponentsByName("Catch").Single() as Entity;
            catchBlock.AddComponent(catchActor);

            var finallyActor = Substitute.For<ActorComponent>();
            var finallyBlock = tryCatchSequence.GetComponentsByName("Finally").Single() as Entity;
            finallyBlock.AddComponent(finallyActor);

            await tryCatchSequence.BeginProcess();

            tryActor.Received(1).Act();
            catchActor.Received(0).Act();
            finallyActor.Received(1).Act();
        }

        /// <summary>
        /// Validate that if one of the components has error in try block catch block is executed. Finally, block is also executed.
        /// </summary>
        [Test]
        public async Task ValidateThatCatchBlockIsExecutedOnErrorAndFinallyBlockIsAlwaysExecuted()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.When(x => x.SetValue<Exception>(Arg.Any<Argument>(), Arg.Any<Exception>())).
                Do(x => { });
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var tryCatchSequence = new TryCatchSequence();
            tryCatchSequence.EntityManager = entityManager;
            tryCatchSequence.ResolveDependencies();

            var tryActor = Substitute.For<ActorComponent>();
            tryActor.When(x => x.Act()).Do(x => { throw new Exception(); });

            var tryBlock = tryCatchSequence.GetComponentsByName("Try").Single() as Entity;
            tryBlock.AddComponent(tryActor);

            var catchActor = Substitute.For<ActorComponent>();
            var catchBlock = tryCatchSequence.GetComponentsByName("Catch").Single() as Entity;
            catchBlock.AddComponent(catchActor);

            var finallyActor = Substitute.For<ActorComponent>();
            var finallyBlock = tryCatchSequence.GetComponentsByName("Finally").Single() as Entity;
            finallyBlock.AddComponent(finallyActor);

            await tryCatchSequence.BeginProcess();

            tryActor.Received(1).Act();
            catchActor.Received(1).Act();
            finallyActor.Received(1).Act();
        }
    }
}
