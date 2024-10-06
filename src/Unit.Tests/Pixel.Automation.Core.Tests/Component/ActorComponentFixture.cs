using NUnit.Framework;
using System.Threading.Tasks;

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

        public override async Task ActAsync()
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
            await Task.CompletedTask;
        }
    }

    [TestFixture]
    public class ActorComponentFixture
    {
        [Test]
        public void ValidateThatActorComponentCanBeInitialized()
        {
            var actorComponent = new FakeActorComponent("Name", "Tag");
            Assert.That(actorComponent is not null);
            Assert.That(actorComponent.Name, Is.EqualTo("Name"));
            Assert.That(actorComponent.Tag, Is.EqualTo("Tag"));
            Assert.That(actorComponent.ContinueOnError == false);
            Assert.That(actorComponent.IsExecuting == false);
            Assert.That(actorComponent.ErrorMessages is not null);
        }

        [Test]
        public async Task ValidateThatActorComponentCanAct()
        {
            var actorComponent = new FakeActorComponent("Name", "Tag");
            await actorComponent.ActAsync();
            Assert.That(actorComponent.IsFaulted);
        }

        [Test]
        public async Task ValidateThatActorComponentCanBeReset()
        {
            var actorComponent = new FakeActorComponent("Name", "Tag");
            await actorComponent.ActAsync();
            Assert.That(actorComponent.IsFaulted);
            actorComponent.ResetComponent();
            Assert.That(actorComponent.IsFaulted == false);
        }
    }
}
