using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class GetValueActorComponentTests
    {
        /// <summary>
        /// Validate that Get value actor can retrieve value attribute from target control
        /// </summary>
        [Test]
        public async Task ValidateThatGetValueActorCanRetrieveValueFromTargetControl()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.When(x => x.SetValueAsync(Arg.Any<Argument>(), Arg.Any<string>()))
                .Do(p =>
                {

                });

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);



            IWebElement targetControl = Substitute.For<IWebElement>();
            targetControl.GetAttribute(Arg.Is<string>("value")).Returns("Sea of Thieves");
            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.GetApiControl<IWebElement>().Returns(targetControl);

            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);

            var getValueActor = new GetValueActorComponent()
            {
                EntityManager = entityManager,
                Parent = controlEntity
            };
            await getValueActor.ActAsync();

            targetControl.Received(1).GetAttribute(Arg.Is<string>("value"));
            _ = targetControl.DidNotReceive().Text;
            argumentProcessor.Received(1).SetValueAsync<string>(Arg.Any<Argument>(), Arg.Is<string>("Sea of Thieves"));
        }


        [Test]
        public async Task ValidateThatGetValueActorReturnsTextIfValueAttributeOfTargetControlIsNullOrEmptyAsync()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.When(x => x.SetValueAsync(Arg.Any<Argument>(), Arg.Any<string>()))
                .Do(p =>
                {

                });

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            IWebElement targetControl = Substitute.For<IWebElement>();
            targetControl.GetAttribute(Arg.Is<string>("value")).Returns("");
            targetControl.Text.Returns("Sea of Thieves");
            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.GetApiControl<IWebElement>().Returns(targetControl);

            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);

            var getValueActor = new GetValueActorComponent()
            {
                EntityManager = entityManager,
                Parent = controlEntity
            };
            await getValueActor.ActAsync();

            targetControl.Received(1).GetAttribute(Arg.Is<string>("value"));
             _ = targetControl.Received(1).Text;
            argumentProcessor.Received(1).SetValueAsync<string>(Arg.Any<Argument>(), Arg.Is<string>("Sea of Thieves"));
        }
    }
}

