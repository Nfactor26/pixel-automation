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
            Assert.AreEqual(1, attributes.Count);

            var attribute = attributes[0];
            Assert.AreEqual(AttributeTargets.Class, attribute.ValidOn);
        }

        [TestCase(typeof(InjectedAttribute))]
        [TestCase(typeof(ParameterUsageAttribute))]   
        public void ValidateThatAttributeHasPropertyUsageFlag(Type attributeType)
        {
            var attributes = (IList<AttributeUsageAttribute>)attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.AreEqual(1, attributes.Count);

            var attribute = attributes[0];
            Assert.AreEqual(AttributeTargets.Property, attribute.ValidOn);
        }


        [TestCase(typeof(BuilderAttribute))]
        [TestCase(typeof(ContainerEntityAttribute))]
        [TestCase(typeof(ControlLocatorAttribute))]
        [TestCase(typeof(FileDescriptionAttribute))]
        [TestCase(typeof(InjectedAttribute))]
        [TestCase(typeof(ParameterUsageAttribute))]      
        [TestCase(typeof(ScriptableAttribute))]
        [TestCase(typeof(ToolBoxItemAttribute))]
        public void ValidateThatAttributeHasAllowMultipleFalse(Type attributeType)
        {
            var attributes = (IList<AttributeUsageAttribute>)attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.AreEqual(1, attributes.Count);

            var attribute = attributes[0];
            Assert.IsFalse(attribute.AllowMultiple);
        }

        [TestCase(typeof(InitializerAttribute))]
        public void ValidateThatAttributeHasAllowMultiplTrue(Type attributeType)
        {
            var attributes = (IList<AttributeUsageAttribute>)attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.AreEqual(1, attributes.Count);

            var attribute = attributes[0];
            Assert.IsTrue(attribute.AllowMultiple);
        }
    }
}
