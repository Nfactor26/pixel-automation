using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class SelectListItemActorComponentTests
    {
        /// <summary>
        /// Validate that select list item can select configured item in list
        /// </summary>
        [TestCase(SelectBy.Text, "Option 2")]
        [TestCase(SelectBy.Index, "2")]
        [TestCase(SelectBy.Value, "Value 2")]
        public void ValidateThatSelectListItemCanSelectConfiguredItemInList(SelectBy selectBy, string option)
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<Argument>()).Returns(option);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);


            IWebElement optionElement = Substitute.For<IWebElement>();
            optionElement.GetAttribute(Arg.Is<string>("index")).Returns("2");

            IWebElement targetControl = Substitute.For<IWebElement>();
            targetControl.TagName.Returns("select");
            targetControl.FindElements(Arg.Any<By>()).Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(new[] { optionElement }));        

            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.GetApiControl<IWebElement>().Returns(targetControl);

            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);

            var selectListItemActor = new SelectListItemActorComponent()
            {
                EntityManager = entityManager,
                Parent = controlEntity,
                SelectBy = selectBy
            };
            selectListItemActor.Act();

            targetControl.Received(1).FindElements(Arg.Any<By>());
            optionElement.Received(1).Click();
           
        }
    }
}
