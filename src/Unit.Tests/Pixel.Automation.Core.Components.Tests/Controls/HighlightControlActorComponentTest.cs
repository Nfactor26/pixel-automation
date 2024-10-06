using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Controls;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class HighlightControlActorComponentTest
    {

        [Test]
        public void VerifyThatFindControlActorBuilderCanBuildFindControlActor()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var controlBuilder = new HighlightControlActorBuilder();
            var containerEntity = controlBuilder.CreateComponent() as GroupEntity;
            containerEntity.EntityManager = entityManager;
            containerEntity.ResolveDependencies();

            Assert.That(containerEntity.GroupPlaceHolder is not null);
            Assert.That(containerEntity.GroupActor is not null);
            Assert.That(containerEntity.GroupActor.GetType(), Is.EqualTo(typeof(HighlightControlActorComponent)));
            Assert.That(containerEntity.Components.Contains(containerEntity.GroupPlaceHolder));
        }

        /// <summary>
        /// Verify that HighlightControlActor can highlight control once it is located given a control entity
        /// Control 
        /// </summary>
        [Test]
        public async Task AssertThatHighlightControlCanFindAndHighlightControlEntity()
        {
            var entityManager = Substitute.For<IEntityManager>();

            IHighlightRectangle highlightRectangle = Substitute.For<IHighlightRectangle>();
            entityManager.GetServiceOfType<IHighlightRectangle>().Returns(highlightRectangle);

            var controlEntity = Substitute.For<IControlEntity>();
            var uiControl = Substitute.For<UIControl>();
            var boundingBox = new BoundingBox(0, 0, 100, 100);
            uiControl.GetBoundingBoxAsync().Returns<BoundingBox>(boundingBox);
            controlEntity.GetControl().Returns(uiControl);

            var componentBuilder = new HighlightControlActorBuilder();
            var containerEntity = componentBuilder.CreateComponent() as GroupEntity;
            containerEntity.EntityManager = entityManager;
            containerEntity.ResolveDependencies();
            containerEntity.GroupPlaceHolder.AddComponent(controlEntity);
          
            await containerEntity.GroupActor.ActAsync();       

            Assert.That(highlightRectangle.Location, Is.EqualTo(boundingBox));
            Assert.That(highlightRectangle.Visible, Is.EqualTo(false));

        }
    }
}
