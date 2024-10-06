using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Loops;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class BreakLoopActorComponentTest
    {
        [Test]
        public async Task ValidateThatLoopEntityExitsOnProcessingBreakLoopActorComponent()
        {

            var loopEntity = Substitute.For<Entity, ILoop>();
            (loopEntity as ILoop).ExitCriteriaSatisfied = false;

            var breakLoopActorComponent = new BreakLoopActorComponent();
            breakLoopActorComponent.Parent = loopEntity;

            //As the loop will be processed, eventually  break loop actor will be processed which will set exitcritiriasatisifed to true 
            //causing the loop to stop
            await breakLoopActorComponent.ActAsync();

            Assert.That((loopEntity as ILoop).ExitCriteriaSatisfied);
        }

        [Test]
        public async Task ValidateThatBreakLoopActorComponentThrowsExceptionIfAddedOutsideALoopConstruct()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var entity = Substitute.ForPartsOf<Entity>();
            entity.EntityManager = entityManager;

            var breakLoopActorComponent = new BreakLoopActorComponent();
            entity.AddComponent(breakLoopActorComponent);

            Assert.ThrowsAsync<InvalidOperationException>(async () => { await breakLoopActorComponent.ActAsync(); });
            await Task.CompletedTask;
        }
    }
}
