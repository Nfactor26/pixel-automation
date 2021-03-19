using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Loops;
using Pixel.Automation.Core.Interfaces;
using System;

namespace Pixel.Automation.Core.Components.Tests
{
    public class BreakLoopActorComponentTest
    {
        [Test]
        public void ValidateThatLoopEntityExitsOnProcessingBreakLoopActorComponent()
        {

            var loopEntity = Substitute.For<Entity, ILoop>();
            (loopEntity as ILoop).ExitCriteriaSatisfied = false;

            var breakLoopActorComponent = new BreakLoopActorComponent();
            breakLoopActorComponent.Parent = loopEntity;

            //As the loop will be processed, eventually  break loop actor will be processed which will set exitcritiriasatisifed to true 
            //causing the loop to stop
            breakLoopActorComponent.Act();

            Assert.IsTrue((loopEntity as ILoop).ExitCriteriaSatisfied);
        }

        [Test]
        public void ValidateThatBreakLoopActorComponentThrowsExceptionIfAddedOutsideALoopConstruct()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var entity = Substitute.ForPartsOf<Entity>();
            entity.EntityManager = entityManager;

            var breakLoopActorComponent = new BreakLoopActorComponent();
            entity.AddComponent(breakLoopActorComponent);

            Assert.Throws<InvalidOperationException>(() => { breakLoopActorComponent.Act(); });
        }
    }
}
