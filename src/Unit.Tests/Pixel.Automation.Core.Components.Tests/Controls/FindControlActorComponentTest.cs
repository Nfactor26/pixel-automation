using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components.Controls;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class FindControlActorComponentTest
    {
        [Test]
        public void VerifyThatFindControlActorBuilderCanBuildFindControlActor()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var findAllActorComponentBuilder = new FindControlActorBuilder();
            var containerEntity = findAllActorComponentBuilder.CreateComponent() as GroupEntity;
            containerEntity.EntityManager = entityManager;
            containerEntity.ResolveDependencies();

            Assert.That(containerEntity.GroupPlaceHolder is not null);
            Assert.That(containerEntity.GroupActor is not null);
            Assert.That(containerEntity.GroupActor.GetType(), Is.EqualTo(typeof(FindControlActorComponent)));
            Assert.That(containerEntity.Components.Contains(containerEntity.GroupPlaceHolder));
        }

        [Test]
        public async Task AssertThatFindControlActorCanLocateMatchingControl()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var controlEntity = Substitute.For<IControlEntity>();
            var uiControl = Substitute.For<UIControl>();
            controlEntity.GetControl().Returns(uiControl);

            var findControlActorComponentBuilder = new FindControlActorBuilder();
            var containerEntity = findControlActorComponentBuilder.CreateComponent() as GroupEntity;
            containerEntity.EntityManager = entityManager;
            containerEntity.ResolveDependencies();
            containerEntity.GroupPlaceHolder.AddComponent(controlEntity);

            UIControl foundControl = default;
            bool wasLocated = false;

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.When(x => x.SetValueAsync(Arg.Any<Argument>(), Arg.Any<UIControl>()))
                .Do(p =>
                {
                    foundControl = p.ArgAt<UIControl>(1);
                });
            argumentProcessor.When(x => x.SetValueAsync(Arg.Any<Argument>(), Arg.Any<bool>()))
                .Do(p =>
                {
                    wasLocated = p.ArgAt<bool>(1);
                });

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            await containerEntity.GroupActor.ActAsync();


            Assert.That(foundControl, Is.EqualTo(uiControl));
            Assert.That(wasLocated, Is.EqualTo(true));

        }
    }
}
