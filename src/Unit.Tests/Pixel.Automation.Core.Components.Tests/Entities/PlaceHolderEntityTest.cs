using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Linq;

namespace Pixel.Automation.Core.Components.Tests
{
    public class PlaceHolderEntityTest
    {
        /// <summary>
        /// Validate that only type of components that are configured to be allowed can be added to a place holder entity
        /// </summary>
        [Test]
        public void ValidateThatOnlyAllowedComponentsCanBeAddedToPlaceHolderEntity()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var placeHolderEntity = new PlaceHolderEntity()
            {
                AllowedComponentsType = typeof(IControlEntity).Name,
                MaxComponentsCount = 5,
                EntityManager = entityManager
            };

            var entity = Substitute.For<IControlEntity>();
            var actorComponent = Substitute.For<ActorComponent>();

            placeHolderEntity.AddComponent(entity);
            Assert.That(placeHolderEntity.Components.Count(), Is.EqualTo(1));

            Assert.Throws<ArgumentException>(() => { placeHolderEntity.AddComponent(actorComponent); });

        }

        /// <summary>
        /// Validate that only up to maximum number of configured componets can be added to a place holder entity
        /// </summary>
        [Test]
        public void ValidateThatOnlyMaxAllowedNumberOfComponentsCanBeAddedToPlaceHolderEntity()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var placeHolderEntity = new PlaceHolderEntity()
            {
                AllowedComponentsType = typeof(IComponent).Name,
                MaxComponentsCount = 3,
                EntityManager = entityManager
            };

            var entity = Substitute.For<Entity>();
            var actorComponent = Substitute.For<ActorComponent>();
            var component = Substitute.For<Component>();
            var asyncActorComponent = Substitute.For<ActorComponent>();

            placeHolderEntity.AddComponent(entity);
            placeHolderEntity.AddComponent(actorComponent);
            placeHolderEntity.AddComponent(component);
            Assert.That(placeHolderEntity.Components.Count(), Is.EqualTo(3));

            Assert.Throws<InvalidOperationException>(() => { placeHolderEntity.AddComponent(asyncActorComponent); });

        }
    }
}
