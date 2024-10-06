using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components.Tests
{
    public class WebControlLocatorComponentTests
    {
        private WebControlLocatorComponent webControlLocator;
        private IWebDriver webDriver;
        private IWebElement controlOne;
        private IWebElement controlTwo;

        [SetUp]
        public void Setup()
        {
            webDriver = Substitute.For<IWebDriver, IJavaScriptExecutor>();


            controlOne = Substitute.For<IWebElement>();
            controlTwo = Substitute.For<IWebElement>();
            webDriver.FindElements(Arg.Any<By>()).Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(new[] { controlOne, controlTwo }));

            webDriver.FindElement(Arg.Any<By>()).Returns(controlOne);

            (webDriver as IJavaScriptExecutor).ExecuteScript(Arg.Any<string>(), Arg.Any<object[]>()).Returns(false);
            (webDriver as IJavaScriptExecutor).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>()).Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(new[] { controlOne, controlTwo }));

            WebApplication webapplication = new WebApplication()
            {
                WebDriver = webDriver
            };

            var entityManager = Substitute.For<IEntityManager>();
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webapplication);
         
            webControlLocator = new WebControlLocatorComponent()
            {
                EntityManager = entityManager
            };
        }
            
        [TearDown]
        public void CleanUp()
        {
            webDriver.ClearReceivedCalls();
        }


        [Test]
        public void ValidateThatWebControlLocatorCanOnlyProcessTypeWebControl()
        {
            WebControlLocatorComponent controlLocator = new WebControlLocatorComponent();
            WebControlIdentity webControlIdentity = new WebControlIdentity();
            var controlIdentity = Substitute.For<IControlIdentity>();

            Assert.That(controlLocator.CanProcessControlOfType(webControlIdentity));
            Assert.That(controlLocator.CanProcessControlOfType(controlIdentity) == false);
        }
        


        [TestCase("#sbformq", "Id")]
        [TestCase("#sbformq", "ClassName")]
        [TestCase("#sbformq", "CssSelector")]
        [TestCase("#sbformq", "LinkText")]
        [TestCase("#sbformq", "Name")]
        [TestCase("#sbformq", "PartialLinkText")]
        [TestCase("#sbformq", "TagName")]
        [TestCase("#sbformq", "XPath")]        
        public void ValidateThatControlLocatorCanFindDescendantControl(string identifier, string findBy)
        {             

            var controlIdentity = new WebControlIdentity()
            {             
                Identifier = identifier,
                FindByStrategy = findBy,
            };

            var locatedControl = webControlLocator.FindDescendantControl(controlIdentity, webDriver);

            Assert.That(locatedControl is not null);
            Assert.That(locatedControl, Is.EqualTo(controlOne));

            webDriver.Received(1).FindElement(Arg.Any<By>());
        }

        [TestCase("#sbformq", "Id")]
        [TestCase("#sbformq", "ClassName")]
        [TestCase("#sbformq", "CssSelector")]
        [TestCase("#sbformq", "LinkText")]
        [TestCase("#sbformq", "Name")]
        [TestCase("#sbformq", "PartialLinkText")]
        [TestCase("#sbformq", "TagName")]
        [TestCase("#sbformq", "XPath")]
        public void ValidateThatControlLocatorCanFindAllDescendantControl(string identifier, string findBy)
        {        

            var controlIdentity = new WebControlIdentity()
            {               
                Identifier = identifier,
                FindByStrategy = findBy,
            };

            var locatedControls = webControlLocator.FindAllDescendantControls(controlIdentity, webDriver);

            Assert.That(locatedControls is not null);
            Assert.That(locatedControls.Count, Is.EqualTo(2));

            webDriver.Received(1).FindElements(Arg.Any<By>());
        }
        

        [TestCase("#sbformq", "Id")]
        [TestCase("#sbformq", "ClassName")]
        [TestCase("#sbformq", "CssSelector")]     
        [TestCase("#sbformq", "Name")]    
        [TestCase("#sbformq", "TagName")]
        [TestCase("#sbformq", "XPath")]
        public void ValidateThatControlLocatorCanFindSiblingControl(string identifier, string findBy)
        {

            var entityManager = Substitute.For<IEntityManager>();          
            var webDriver = Substitute.For<IWebDriver, IJavaScriptExecutor>();
            var webElement = Substitute.For<IWebElement>();          
           
            (webDriver as IJavaScriptExecutor).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>()).Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(new[] { webElement }));

            //for XPath
            webDriver.FindElements(Arg.Any<By>()).Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(new[] { webElement }));

            WebApplication webapplication = new WebApplication()
            {
                WebDriver = webDriver
            };

            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webapplication);

            WebControlLocatorComponent controlLocator = new WebControlLocatorComponent()
            {
                EntityManager = entityManager
            };

            var controlIdentity = new WebControlIdentity()
            {               
                Identifier = identifier,
                FindByStrategy = findBy,
            };

            var locatedControl = controlLocator.FindSiblingControl(controlIdentity, webDriver);

            Assert.That(locatedControl is not null);
            Assert.That(locatedControl, Is.SameAs(webElement));


            switch (findBy)
            {
                case "XPath":
                    webDriver.Received(1).FindElements(Arg.Any<By>());
                    break;
                default:
                    (webDriver as IJavaScriptExecutor).Received(1).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>());
                    break;
            }
        }

        [Test]
        public void ValidateThatControlLocatorThrowsExceptionIfMoreThanOneControlAreLocatedForFindSiblingControl()
        {
            var controlIdentity = new WebControlIdentity()
            {              
                Identifier = "sbformq",
                FindByStrategy = "Id",
            };

            Assert.Throws<ArgumentException>(() => { webControlLocator.FindSiblingControl(controlIdentity, webDriver); }) ;           
         
            (webDriver as IJavaScriptExecutor).Received(1).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>());
        }

        [TestCase("#sbformq", "Id")]
        [TestCase("#sbformq", "ClassName")]
        [TestCase("#sbformq", "CssSelector")]
        [TestCase("#sbformq", "Name")]
        [TestCase("#sbformq", "TagName")]
        [TestCase("#sbformq", "XPath")]
        public void ValidateThatControlLocatorCanFindAllSiblingControls(string identifier, string findBy)
        {
            var controlIdentity = new WebControlIdentity()
            {
                Identifier = identifier,
                FindByStrategy = findBy,
            };
        
            var locatedControls = webControlLocator.FindAllSiblingControls(controlIdentity, webDriver);

            Assert.That(locatedControls is not null);
            Assert.That(locatedControls.Count, Is.EqualTo(2));
            
            switch(findBy)
            {
                case "XPath":
                    webDriver.Received(1).FindElements(Arg.Any<By>());
                    break;
                default:
                    (webDriver as IJavaScriptExecutor).Received(1).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>());
                    break;
            }

        }

        [TestCase("#sbformq", "LinkText")]
        [TestCase("#sbformq", "PartialLinkText")]
        public void ValidateThatControlLocatorThrowsExceptionForUnSupportedFindByStrategiesForFindAllSiblingControls(string identifier, string findBy)
        {
            var controlIdentity = new WebControlIdentity()
            {             
                Identifier = identifier,
                FindByStrategy = findBy,
            };

            Assert.Throws<NotSupportedException>(() => { webControlLocator.FindAllSiblingControls(controlIdentity, webDriver); });
          
        }


        [TestCase("#sbformq", "Id")]
        [TestCase("#sbformq", "ClassName")]
        [TestCase("#sbformq", "CssSelector")]
        [TestCase("#sbformq", "Name")]
        [TestCase("#sbformq", "TagName")]
        [TestCase("#sbformq", "XPath")]
        public void ValidateThatControlLocatorCanLocateAncestorControl(string identifier, string findBy)
        {
            var webDriver = Substitute.For<IWebDriver, IJavaScriptExecutor>();
            var webElement = Substitute.For<IWebElement>();         
            (webDriver as IJavaScriptExecutor).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>()).Returns(webElement);

            //for XPath
            webDriver.FindElement(Arg.Any<By>()).Returns(webElement);

            WebApplication webapplication = new WebApplication()
            {
                WebDriver = webDriver
            };

            var entityManager = Substitute.For<IEntityManager>();
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webapplication);

            WebControlLocatorComponent controlLocator = new WebControlLocatorComponent()
            {
                EntityManager = entityManager
            };

            var controlIdentity = new WebControlIdentity()
            {
                Identifier = identifier,
                FindByStrategy = findBy,
            };

            var locatedControl = controlLocator.FindAncestorControl(controlIdentity, webDriver);

            Assert.That(locatedControl is not null);
            Assert.That(locatedControl, Is.EqualTo(webElement));

            switch (findBy)
            {
                case "XPath":
                    webDriver.Received(1).FindElement(Arg.Any<By>());
                    break;
                default:
                    (webDriver as IJavaScriptExecutor).Received(1).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>());
                    break;
            }

        }


        [TestCase(Core.Enums.SearchScope.Descendants)]
        [TestCase(Core.Enums.SearchScope.Children)]
        [TestCase(Core.Enums.SearchScope.Sibling)]
        [TestCase(Core.Enums.SearchScope.Ancestor)]
        public async Task ValidateThatControlLocatorCanFindControl(Core.Enums.SearchScope searchScope)
        {

            var entityManager = Substitute.For<IEntityManager>();
            var webDriver = Substitute.For<IWebDriver, IJavaScriptExecutor>();
            var webElement = Substitute.For<IWebElement>();
        
            //for SearchScope.Descendants
            webDriver.FindElement(Arg.Any<By>()).Returns(webElement);

           switch(searchScope)
            {
                case Core.Enums.SearchScope.Ancestor:
                    (webDriver as IJavaScriptExecutor).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>()).Returns(webElement);
                    break;
                case Core.Enums.SearchScope.Sibling:
                    (webDriver as IJavaScriptExecutor).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>()).Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(new[] { webElement }));
                    break;
            }
           
         
            WebApplication webapplication = new WebApplication()
            {
                WebDriver = webDriver
            };

            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webapplication);

            WebControlLocatorComponent controlLocator = new WebControlLocatorComponent()
            {
                EntityManager = entityManager
            };


            var controlIdentity = new WebControlIdentity()
            {
                SearchScope = searchScope,
                Identifier = "sbformq",
                FindByStrategy = "Id",
            };

            switch(searchScope)
            {
                case Core.Enums.SearchScope.Descendants:
                    var locatedControl = await controlLocator.FindControlAsync(controlIdentity);
                    Assert.That(locatedControl is not null);
                    Assert.That(locatedControl.GetApiControl<IWebElement>(), Is.EqualTo(webElement));
                    webDriver.Received(1).FindElement(Arg.Any<By>());
                    break;
                case Core.Enums.SearchScope.Children:
                    Assert.ThrowsAsync<NotSupportedException>(async () => { await controlLocator.FindControlAsync(controlIdentity); });
                    break;
                case Core.Enums.SearchScope.Ancestor:
                case Core.Enums.SearchScope.Sibling:

                    var foundControl = await controlLocator.FindControlAsync(controlIdentity);
                    Assert.That(foundControl is not null);
                    Assert.That(foundControl.GetApiControl<IWebElement>(), Is.EqualTo(webElement));
                    (webDriver as IJavaScriptExecutor).Received(1).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>());
                    break;
            }        
        }


        [TestCase(Core.Enums.SearchScope.Descendants, 2)]
        [TestCase(Core.Enums.SearchScope.Children, 1)]
        [TestCase(Core.Enums.SearchScope.Sibling, 2)]
        [TestCase(Core.Enums.SearchScope.Ancestor, 2)]
        public async Task ValidateThatControlLocatorCanFindControlAtConfiguredIndex(Core.Enums.SearchScope searchScope, int index)
        {
            var controlIdentity = new WebControlIdentity()
            {
                SearchScope = searchScope,
                Identifier = "sbformq",
                FindByStrategy = "Id",
                Index = index
            };

            switch (searchScope)
            {
                case Core.Enums.SearchScope.Descendants:
                    var locatedControl = await webControlLocator.FindControlAsync(controlIdentity);
                    Assert.That(locatedControl is not null);
                    Assert.That(locatedControl.GetApiControl<IWebElement>(), Is.EqualTo(controlTwo));
                    webDriver.Received(1).FindElements(Arg.Any<By>());
                    break;
                case Core.Enums.SearchScope.Children:
                case Core.Enums.SearchScope.Ancestor:
                    Assert.ThrowsAsync<NotSupportedException>(async () => { await webControlLocator.FindControlAsync(controlIdentity); });
                    break;
            
                case Core.Enums.SearchScope.Sibling:
                    var foundControl = await webControlLocator.FindControlAsync(controlIdentity);
                    Assert.That(foundControl is not null);
                    Assert.That(foundControl.GetApiControl<IWebElement>(), Is.EqualTo(controlTwo));
                    (webDriver as IJavaScriptExecutor).Received(1).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>());
                    break;
            }
        }

        [TestCase(Core.Enums.SearchScope.Descendants)]
        [TestCase(Core.Enums.SearchScope.Children)]
        [TestCase(Core.Enums.SearchScope.Sibling)]
        [TestCase(Core.Enums.SearchScope.Ancestor)]

        public async Task ValidateThatControlLocatorCanFindAllControls(Core.Enums.SearchScope searchScope)
        {
            var controlIdentity = new WebControlIdentity()
            {
                SearchScope = searchScope,
                Identifier = "form",
                FindByStrategy = "Id",
                Index = 2,
                Next = new WebControlIdentity()
                {
                    SearchScope = Core.Enums.SearchScope.Descendants,
                    Identifier = "input",
                    FindByStrategy = "Id"                    
                }                
            };

            switch (searchScope)
            {
                case Core.Enums.SearchScope.Descendants:
                    var locatedControls = await webControlLocator.FindAllControlsAsync(controlIdentity);
                    Assert.That(locatedControls is not null);
                    Assert.That(locatedControls.Count(), Is.EqualTo(2));
                    webDriver.Received(2).FindElements(Arg.Any<By>());
                    break;
                case Core.Enums.SearchScope.Children:
                case Core.Enums.SearchScope.Ancestor:
                    Assert.ThrowsAsync<NotSupportedException>(async () => { await webControlLocator.FindAllControlsAsync(controlIdentity); });
                    break;

                case Core.Enums.SearchScope.Sibling:
                    var foundControls = await webControlLocator.FindAllControlsAsync(controlIdentity);
                    Assert.That(foundControls is not null);
                    Assert.That(foundControls.Count(), Is.EqualTo(2));
                    (webDriver as IJavaScriptExecutor).Received(1).ExecuteScript(Arg.Any<string>(), Arg.Any<ISearchContext>(), Arg.Any<string>());
                    break;
            }
        }

    }
}