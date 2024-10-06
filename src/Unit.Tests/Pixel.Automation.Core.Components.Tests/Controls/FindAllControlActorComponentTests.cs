using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components.Controls;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class FindAllControlActorComponentTests
    {
        [Test]
        public void VerifyThatFindAllControlsActorBuilderCanBuildFindAllControlsActor()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var findAllActorComponentBuilder = new FindAllControlsActorBuilder();
            var containerEntity = findAllActorComponentBuilder.CreateComponent() as GroupEntity;
            containerEntity.EntityManager = entityManager;
            containerEntity.ResolveDependencies();

            Assert.That(containerEntity.GroupPlaceHolder is not null);
            Assert.That(containerEntity.GroupActor is not null);
            Assert.That(containerEntity.GroupActor.GetType(), Is.EqualTo(typeof(FindAllControlsActorComponent)));
            Assert.That(containerEntity.Components.Contains(containerEntity.GroupPlaceHolder));
        }

        [Test]
        public async Task AssertThatFindAllControlActorCanLocateAllMatchingControls()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var controlEntity = Substitute.For<IControlEntity>();
            List<UIControl> controls = new List<UIControl>();
            controls.Add(Substitute.For<UIControl>());
            controls.Add(Substitute.For<UIControl>());
            controlEntity.GetAllControls().Returns(controls);

            var findAllControlActorComponentBuilder = new FindAllControlsActorBuilder();
            var containerEntity = findAllControlActorComponentBuilder.CreateComponent() as GroupEntity;
            (containerEntity.GroupActor as FindAllControlsActorComponent).Count = new OutArgument<int>() { Mode = ArgumentMode.DataBound, PropertyPath = "Count" };
            containerEntity.EntityManager = entityManager;
            containerEntity.ResolveDependencies();
            containerEntity.GroupPlaceHolder.AddComponent(controlEntity);

            List<UIControl> foundControls = default;
            int foundControlsCount = 0;

            var argumentProcessor = Substitute.For<IArgumentProcessor>();           
            argumentProcessor.When(x => x.SetValueAsync(Arg.Any<Argument>(), Arg.Any<IEnumerable<UIControl>>()))
                .Do(p => 
                { 
                    foundControls = p.ArgAt<List<UIControl>>(1); 
                });       
            argumentProcessor.When(x => x.SetValueAsync(Arg.Any<Argument>(), Arg.Any<int>()))
                .Do(p =>
                { 
                    foundControlsCount = p.ArgAt<int>(1);
                });

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            await containerEntity.GroupActor.ActAsync();


            Assert.That(foundControls, Is.EqualTo(controls));
            Assert.That(foundControlsCount, Is.EqualTo(2));

        }
    }
}
