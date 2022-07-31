using NUnit.Framework;
using Pixel.Automation.Core.Exceptions;
using System;

namespace Pixel.Automation.Core.Tests.Exceptions
{
    class GenericExceptionFixture
    {
        /// <summary>
        /// Validate that all the custom exceptions defined in Core can be initialized properly
        /// </summary>
        /// <param name="type"></param>
        [TestCase(typeof(ConfigurationException))]
        [TestCase(typeof(ElementNotFoundException))]
        [TestCase(typeof(ArgumentNotConfiguredException))]       
        [TestCase(typeof(MissingComponentException))]          
        public void ValidateThatExceptionObjectsCanBeInitialized(Type type)
        {
            var exception = Activator.CreateInstance(type) as Exception;
            Assert.AreEqual($"Exception of type '{type.Namespace}.{type.Name}' was thrown.", exception.Message);
            Assert.IsNull(exception.InnerException);

            exception = Activator.CreateInstance(type, new object[] { type.Name }) as Exception;
            Assert.AreEqual(type.Name, exception.Message);
            Assert.IsNull(exception.InnerException);

            exception = Activator.CreateInstance(type, new object[] { type.Name, new Exception() }) as Exception;
            Assert.AreEqual(type.Name, exception.Message);
            Assert.IsNotNull(exception.InnerException);
        }
    }
}
