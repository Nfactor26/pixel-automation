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
            Assert.IsNotNull(actorComponent);
            Assert.AreEqual("Name", actorComponent.Name);
            Assert.AreEqual("Tag", actorComponent.Tag);
            Assert.IsFalse(actorComponent.ContinueOnError);
            Assert.IsFalse(actorComponent.IsExecuting);
            Assert.IsNotNull(actorComponent.ErrorMessages);
        }

        [Test]
        public async Task ValidateThatAsyncActorComponentCanAct()
        {
            var actorComponent = new FakeAsyncActorComponent("Name", "Tag");
            await actorComponent.ActAsync();
            Assert.IsTrue(actorComponent.IsFaulted);
        }

        [Test]
        public async Task ValidateThatAsyncActorComponentCanBeReset()
        {
            var actorComponent = new FakeAsyncActorComponent("Name", "Tag");
            await actorComponent.ActAsync();
            Assert.IsTrue(actorComponent.IsFaulted);
            actorComponent.ResetComponent();
            Assert.IsFalse(actorComponent.IsFaulted);
        }
    }
}
