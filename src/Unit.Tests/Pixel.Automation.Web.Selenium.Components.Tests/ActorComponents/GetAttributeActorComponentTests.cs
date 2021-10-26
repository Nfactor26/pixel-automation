using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;


namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class GetAttributeActorComponentTests
    {
        /// <summary>
        /// Validate that Click actor component can perform click on a web element
        /// </summary>
        [Test]
        public void ValidateThatGetAttributeActorCanRetrieveAttributeValueFromTargetControl()
        {
            var entityManager = Substitute.For<IEntityManager>();           
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.When(x => x.SetValue(Arg.Any<Argument>(), Arg.Any<string>()))
                .Do(p =>
                {
                   
                });
     
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);



            IWebElement targetControl = Substitute.For<IWebElement>();
            targetControl.GetAttribute(Arg.Any<string>()).Returns("Enter your search term");
            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.GetApiControl<IWebElement>().Returns(targetControl);

            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);

            var getAttributeActor = new GetAttributeActorComponent()
            {
                EntityManager = entityManager,
                Parent = controlEntity,
                AttributeName = "title"
            };
            getAttributeActor.Act();
          
            targetControl.Received(1).GetAttribute(Arg.Is<string>("title"));
            argumentProcessor.Received(1).SetValue<string>(Arg.Any<Argument>(), Arg.Is<string>("Enter your search term"));
        }
    }
}

