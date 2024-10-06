using NUnit.Framework;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Tests
{
    public class FakeAsyncActorComponent : ActorComponent
    {
        public FakeAsyncActorComponent() : base()
        {

        }

        public FakeAsyncActorComponent(string name, string tag) : base(name, tag)
        {

        }

        public override async Task ActAsync()
        {
            try
            {
                this.IsExecuting = true;
                this.IsFaulted = true;
                await Task.CompletedTask;
            }
            finally
            {
                this.IsExecuting = false;
            }
        }
    }

    [TestFixture]
    public class AsyncActorComponentFixture
    {
        [Test]
        public void ValidateThatAsyncActorComponentCanBeInitialized()
        {
            var actorComponent = new FakeAsyncActorComponent("Name", "Tag");
            Assert.That(actorComponent is not null);
            Assert.That("Name", Is.EqualTo(actorComponent.Name));
            Assert.That("Tag", Is.EqualTo(actorComponent.Tag));
            Assert.That(actorComponent.ContinueOnError == false);
            Assert.That(actorComponent.IsExecuting == false);
            Assert.That(actorComponent.ErrorMessages is not null);
        }

        [Test]
        public async Task ValidateThatAsyncActorComponentCanAct()
        {
            var actorComponent = new FakeAsyncActorComponent("Name", "Tag");
            await actorComponent.ActAsync();
            Assert.That(actorComponent.IsFaulted);
        }

        [Test]
        public async Task ValidateThatAsyncActorComponentCanBeReset()
        {
            var actorComponent = new FakeAsyncActorComponent("Name", "Tag");
            await actorComponent.ActAsync();
            Assert.That(actorComponent.IsFaulted);
            actorComponent.ResetComponent();
            Assert.That(actorComponent.IsFaulted == false);
        }
    }
}
