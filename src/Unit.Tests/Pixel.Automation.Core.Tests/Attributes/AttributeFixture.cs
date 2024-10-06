using NUnit.Framework;
using Pixel.Automation.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Tests.Attributes
{
    class AttributeFixture
    {

        [TestCase(typeof(BuilderAttribute))]
        [TestCase(typeof(ContainerEntityAttribute))]
        [TestCase(typeof(ControlLocatorAttribute))]
        [TestCase(typeof(FileDescriptionAttribute))]
        [TestCase(typeof(InitializerAttribute))]
        [TestCase(typeof(ScriptableAttribute))]
        [TestCase(typeof(ToolBoxItemAttribute))]
        public void ValidateThatAttributeHasClassUsageFlag(Type attributeType)
        {
            var attributes = (IList<AttributeUsageAttribute>)attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.That(attributes.Count, Is.EqualTo(1));

            var attribute = attributes[0];
            Assert.That(attribute.ValidOn, Is.EqualTo(AttributeTargets.Class));
        }

        [TestCase(typeof(InjectedAttribute))]
        [TestCase(typeof(ParameterUsageAttribute))]   
        public void ValidateThatAttributeHasPropertyUsageFlag(Type attributeType)
        {
            var attributes = (IList<AttributeUsageAttribute>)attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.That(attributes.Count, Is.EqualTo(1));

            var attribute = attributes[0];
            Assert.That(attribute.ValidOn, Is.EqualTo(AttributeTargets.Property));
        }


        [TestCase(typeof(BuilderAttribute))]
        [TestCase(typeof(ContainerEntityAttribute))]    
        [TestCase(typeof(FileDescriptionAttribute))]
        [TestCase(typeof(InjectedAttribute))]
        [TestCase(typeof(ParameterUsageAttribute))]      
        [TestCase(typeof(ScriptableAttribute))]
        [TestCase(typeof(ToolBoxItemAttribute))]
        public void ValidateThatAttributeHasAllowMultipleFalse(Type attributeType)
        {
            var attributes = (IList<AttributeUsageAttribute>)attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.That(attributes.Count, Is.EqualTo(1));

            var attribute = attributes[0];
            Assert.That(attribute.AllowMultiple == false);
        }

        [TestCase(typeof(InitializerAttribute))]
        [TestCase(typeof(ControlLocatorAttribute))]
        public void ValidateThatAttributeHasAllowMultiplTrue(Type attributeType)
        {
            var attributes = (IList<AttributeUsageAttribute>)attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.That(attributes.Count, Is.EqualTo(1));

            var attribute = attributes[0];
            Assert.That(attribute.AllowMultiple);
        }
    }
}
