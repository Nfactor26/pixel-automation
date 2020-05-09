using NUnit.Framework;
using System;
using System.Collections.Generic;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Test.Helpers;

namespace Pixel.Automation.Core.Tests
{
    public class TypeExtensionTests
    {
        /// <summary>
        /// Validate that GetDisplayName extension method returns the correct simplified display name
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="expectedDisplayName"></param>
        [TestCase(typeof(Console), "Console")]
        [TestCase(typeof(int), "Int32")]
        [TestCase(typeof(string), "String")]
        [TestCase(typeof(List<string>), "List<String>")]
        [TestCase(typeof(Dictionary<int,string>), "Dictionary<Int32, String>")]
        [TestCase(typeof(IEnumerable<>), "IEnumerable")]
        public void CanGetCorrectTypeDisplayName(Type targetType, string expectedDisplayName)
        {
            Assert.AreEqual(expectedDisplayName, targetType.GetDisplayName());
        }

        /// <summary>
        /// Validate that specified property value can be fetched from object
        /// </summary>
        [Test]
        public void CanGetPropertyValue()
        {

            Person person = new Person()
            {
                Name = "Naruto",
                Age = 16,
                Address = new Address()
                {
                    City = "Konoha",
                    Country = "Hidden Leaf"
                }
            };

            Assert.AreEqual(person.GetPropertyValue<string>("Name"), "Naruto");
            Assert.AreEqual(person.GetPropertyValue<int>("Age"), 16);
            Assert.IsTrue(person.GetPropertyValue<Address>("Address").City.Equals("Konoha"));

        }

        /// <summary>
        /// Validate that an attempt to request a  type not compatible with actualy property type throws an exception
        /// </summary>
        [Test] 
        public void GetPropertyValueShouldThrowExcpetionIfRequestTypeIsIncompatibleWithActualType()
        {

            Person person = new Person()
            {
                Age = 16               
            };

            Assert.Throws<Exception>(() => person.GetPropertyValue<string>("Age"));          
        }

        /// <summary>
        /// Validate that property value can be set on an object
        /// </summary>
        [Test]
        public void CanSetPropertyValue()
        {
            Person person = new Person()
            {
                Name = "Naruto",
                Age = 16,
                Address = new Address()
                {
                    City = "Konoha",
                    Country = "Hidden Leaf"
                }
            };


            person.SetPropertyValue<string>("Name", "Gaara");
            person.SetPropertyValue<int>("Age", 15);
            person.Address.SetPropertyValue<string>("Country", "Hidden Sand");

            Assert.AreEqual(person.GetPropertyValue<string>("Name"), "Gaara");
            Assert.AreEqual(person.GetPropertyValue<int>("Age"), 15);
            Assert.IsTrue(person.GetPropertyValue<Address>("Address").Country.Equals("Hidden Sand"));
        }


        /// <summary>
        /// Validate that an attempt to set a property value to a type not compatible with property type throws an exception
        /// </summary>
        [Test]
        public void SetPropertyValueShouldThrowExcpetionIfSpecifiedTypeIsIncompatibleWithActualType()
        {

            Person person = new Person()
            {
                Age = 16
            };

            Assert.Throws<Exception>(() => person.SetPropertyValue<string>("Age", "16"));
        }
    }
}