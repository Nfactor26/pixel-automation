using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Test.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Pixel.Automation.Core.Tests.Extensions
{
    [DataContract]
    [Serializable]
    class DummyModel
    {
        [DataMember]
        [Display(Name = "Property One", GroupName = "Group One", Order = 10, AutoGenerateField = false)]
        [Browsable(false)]
        [ReadOnly(false)]
        public string PropertyOne { get; set; }
    }

    class ComponentExtensionsFixture
    {          
        [Test]
        public void ValidateThatDisplayAttributeCanBeToggled()
        {
            var model = new DummyModel();

            Assert.That(GetDisplayAttribute(model, "PropertyOne") == false);
            model.SetDispalyAttribute("PropertyOne", true);
            Assert.That(GetDisplayAttribute(model, "PropertyOne"));

            bool GetDisplayAttribute(object component, string propertyName)
            {
                var attr = TypeDescriptor.GetProperties(component.GetType())[propertyName]?.Attributes[typeof(DisplayAttribute)] as DisplayAttribute;
                return attr.AutoGenerateField;
            }
        }

        [Test]
        public void ValidateThatScopedParentCanBeLocatedWhenPresent()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var actorComponent = new MockActorComponent();
            var entityComponent = new Entity();
            var scopedEntity = new MockWhileLoopEntity();

            scopedEntity.EntityManager = entityManager;          
            scopedEntity.AddComponent(entityComponent);
            entityComponent.AddComponent(actorComponent);

            bool found = actorComponent.TryGetScopedParent(out IScopedEntity scopedParentEntity);

            Assert.That(found);
            Assert.That(scopedEntity, Is.SameAs(scopedParentEntity));
        }

        [Test]
        public void ValidateThatScopedParentCanNotBeLocatedWhenNotPresent()
        {
            var entitManager = Substitute.For<IEntityManager>();
            var actorComponent = new MockActorComponent();
            var entityComponent = new Entity();

            entityComponent.EntityManager = entitManager;         
            entityComponent.AddComponent(actorComponent);

            bool found = actorComponent.TryGetScopedParent(out IScopedEntity scopedParentEntity);

            Assert.That(found == false);
            Assert.That(scopedParentEntity is null);
        }
    }
}
