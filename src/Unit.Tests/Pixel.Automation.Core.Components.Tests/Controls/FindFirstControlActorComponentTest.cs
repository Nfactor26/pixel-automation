using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components.Controls;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Core.Components.Tests
{
    public class FindFirstControlActorComponentTest
    {
        [Test]
        public void VerifyThatFindFirstControlActorBuilderCanBuildFindFirstControlActor()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var findAllActorComponentBuilder = new FindFirstControlActorBuider();
            var containerEntity = findAllActorComponentBuilder.CreateComponent() as GroupEntity;
            containerEntity.EntityManager = entityManager;
            containerEntity.ResolveDependencies();

            Assert.IsNotNull(containerEntity.GroupPlaceHolder);
            Assert.IsNotNull(containerEntity.GroupActor);
            Assert.AreEqual(typeof(FindFirstControlActorComponent), containerEntity.GroupActor.GetType());
            Assert.IsTrue(containerEntity.Components.Contains(containerEntity.GroupPlaceHolder));
        }


        /// <summary>
        /// Given multiple controls, try to locate them and return the first one that could be located.
        /// We will simulate scenario using index in this test case where index will decide which control is available
        /// </summary>
        /// <param name="index"></param>
        [TestCase(0)]
        [TestCase(1)]
        public void AssertThatFindFirstControlActorCanLocateMatchingControl(int index)
        {
            var entityManager = Substitute.For<IEntityManager>();

            var firstControlEntity = Substitute.For<IControlEntity>();
            var firstControlDetails = Substitute.For<IControlIdentity>();
            firstControlEntity.ControlDetails.Returns(firstControlDetails);
            var firstUiControl = Substitute.For<UIControl>();
            if(index == 0)
            {
                firstControlEntity.GetControl().Returns(firstUiControl);
            }
            else
            {
                firstControlEntity.When(x => x.GetControl()).Do(x => { throw new System.Exception(); });
            }


            var secondControlEntity = Substitute.For<IControlEntity>();
            var secondControlDetails = Substitute.For<IControlIdentity>();
            secondControlEntity.ControlDetails.Returns(firstControlDetails);
            var secondUIControl = Substitute.For<UIControl>();
            if (index == 1)
            {
                secondControlEntity.GetControl().Returns(secondUIControl);
            }
            else
            {
                secondControlEntity.When(x => x.GetControl()).Do(x => { throw new System.Exception(); });
            }


            var findFirstControlActorComponentBuilder = new FindFirstControlActorBuider();
            var containerEntity = findFirstControlActorComponentBuilder.CreateComponent() as GroupEntity;
            containerEntity.EntityManager = entityManager;
            containerEntity.ResolveDependencies();
            containerEntity.GroupPlaceHolder.AddComponent(firstControlEntity);
            containerEntity.GroupPlaceHolder.AddComponent(secondControlEntity);
      
            UIControl foundControl = default;
            bool wasLocated = false;

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.When(x => x.SetValue(Arg.Any<Argument>(), Arg.Any<UIControl>()))
                .Do(p =>
                {
                    foundControl = p.ArgAt<UIControl>(1);
                });
            argumentProcessor.When(x => x.SetValue(Arg.Any<Argument>(), Arg.Any<bool>()))
                .Do(p =>
                {
                    wasLocated = p.ArgAt<bool>(1);
                });

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            containerEntity.GroupActor.Act();

            if(index == 0)
            {
                Assert.AreEqual(firstUiControl, foundControl);
            }
            if(index == 1)
            {
                Assert.AreEqual(secondUIControl, foundControl);
            }
            Assert.AreEqual(true, wasLocated);

        }

        [TestCase(false)]
        [TestCase(true)]
        public void GivenDifferentValuesofThrowIfNotFoundAssertThatFindFirstControlActorBehavesAccordingly(bool throwIfNotFound)
        {
            var entityManager = Substitute.For<IEntityManager>();

            var controlEntity = Substitute.For<IControlEntity>();
            var controlDetails = Substitute.For<IControlIdentity>();
            controlEntity.ControlDetails.Returns(controlDetails);
            var firstUiControl = Substitute.For<UIControl>();
            controlEntity.When(x => x.GetControl()).Do(x => { throw new System.Exception(); });

            var findFirstControlActorComponentBuilder = new FindFirstControlActorBuider();
            var containerEntity = findFirstControlActorComponentBuilder.CreateComponent() as GroupEntity;
            (containerEntity.GroupActor as FindFirstControlActorComponent).ThrowIfNotFound = throwIfNotFound;
            containerEntity.EntityManager = entityManager;
            containerEntity.ResolveDependencies();
            containerEntity.AddComponent(controlEntity);      

            bool wasLocated = false;

            var argumentProcessor = Substitute.For<IArgumentProcessor>();       
            argumentProcessor.When(x => x.SetValue(Arg.Any<Argument>(), Arg.Any<bool>()))
                .Do(p =>
                {
                    wasLocated = p.ArgAt<bool>(1);
                });

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
                       
           
            if(throwIfNotFound)
            {
                Assert.Throws<ElementNotFoundException>(() => { containerEntity.GroupActor.Act(); });
            }
            else
            {
                containerEntity.GroupActor.Act();               
            }

            Assert.AreEqual(false, wasLocated);
        }
    }
}
