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
    class SendKeyActorComponentTests
    {
        /// <summary>
        /// Validate that SendKeyActor can set text on control
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatSendKeyActorCanSetText(bool clearBeforeSendKeys)
        {
            var entityManager = Substitute.For<IEntityManager>();

            IWebElement targetControl = Substitute.For<IWebElement>();
            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.GetApiControl<IWebElement>().Returns(targetControl);

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<string>(Arg.Any<InArgument<string>>()).Returns("How you doing?");

            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
          
            var sendKeyActor = new SendKeyActorComponent()
            {
                EntityManager = entityManager,
                Parent = controlEntity,
                ClearBeforeSendKeys = clearBeforeSendKeys
            };
            await sendKeyActor.ActAsync();

            argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            if(clearBeforeSendKeys)
            {
                targetControl.Received(1).Clear();
            }
            else
            {
                targetControl.DidNotReceive().Clear();
            }
            targetControl.Received(1).SendKeys(Arg.Any<string>());
        }
    }
}
