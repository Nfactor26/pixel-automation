using NUnit.Framework;

namespace Pixel.Automation.Core.Tests
{
    public class FakeActorComponent : ActorComponent
    {
        public FakeActorComponent() : base()
        {

        }

        public FakeActorComponent(string name, string tag) : base(name, tag)
        {

        }

        public override void Act()
        {
            try
            {
                this.IsExecuting = true;
                this.IsFaulted = true;
            }
            finally
            {
                this.IsExecuting = false;    
            }
        }
    }

    [TestFixture]
    public class ActorComponentFixture
    {
        [Test]
        public void ValidateThatActorComponentCanBeInitialized()
        {
            var actorComponent = new FakeActorComponent("Name", "Tag");
            Assert.IsNotNull(actorComponent);
            Assert.AreEqual("Name", actorComponent.Name);
            Assert.AreEqual("Tag", actorComponent.Tag);
            Assert.IsFalse(actorComponent.ContinueOnError);
            Assert.IsFalse(actorComponent.IsExecuting);
            Assert.IsNotNull(actorComponent.ErrorMessages);
        }

        [Test]
        public void ValidateThatActorComponentCanAct()
        {
            var actorComponent = new FakeActorComponent("Name", "Tag");
            actorComponent.Act();
            Assert.IsTrue(actorComponent.IsFaulted);
        }

        [Test]
        public void ValidateThatActorComponentCanBeReset()
        {
            var actorComponent = new FakeActorComponent("Name", "Tag");
            actorComponent.Act();
            Assert.IsTrue(actorComponent.IsFaulted);
            actorComponent.ResetComponent();
            Assert.IsFalse(actorComponent.IsFaulted);
        }
    }
}
