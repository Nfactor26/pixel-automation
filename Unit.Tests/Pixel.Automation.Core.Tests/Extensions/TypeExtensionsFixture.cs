using NUnit.Framework;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Test.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Pixel.Automation.Core.Tests.Extensions
{
    class TypeExtensionsFixture
    {
        /// <summary>
        /// Given a type , TypeExtensions has GetDisplayName(Type) method which should be able to correctly generate display name from type
        /// </summary>
        [TestCase(typeof(Console), "Console")]
        [TestCase(typeof(int), "Int32")]
        [TestCase(typeof(string), "String")]
        [TestCase(typeof(Person), "Person")]
        [TestCase(typeof(List<string>), "List<String>")]
        [TestCase(typeof(Dictionary<string, Person>), "Dictionary<String, Person>")]
        [TestCase(typeof(List<List<string>>), "List<List<String>>")]
        [TestCase(typeof(List<>), "List")]
        [TestCase(typeof(IEnumerable<>), "IEnumerable")]
        public void ValidateThatCorrectTypeDisplayNameIsGenerated(Type type, string expectedDisplayName)
        {
            var displayName = type.GetDisplayName();
            Assert.AreEqual(expectedDisplayName, displayName);
        }

        /// <summary>
        /// Validate that GetPropertyValue<T>() can be used to retrieve value of a given property from an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="expectedValue"></param>
        [Test]
        public void ValidateThatPropertyValueCanBeRetrievedByNameFromAnObject<T>(string propertyName, T expectedValue)
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
            Assert.AreEqual(person.GetPropertyValue<string>("Age"), "16"); //integer is assignable to string
            Assert.IsTrue(person.GetPropertyValue<Address>("Address").City.Equals("Konoha"));
        }

        /// <summary>
        /// Validate that ArgumentException is thrown if requested property doesn't exists on object
        /// </summary>
        [Test]
        public void ValidateThatArgumentExceptionIsThrownIfPropertyDoesNotExistOnObject()
        {
            Person person = new Person() { Name = "Luffy", Age = 16 };
            Assert.Throws<ArgumentException>(() => { person.GetPropertyValue<string>("LastName"); });
        }

        /// <summary>
        /// Validate that InvalidCastException is thrown if property value is not assignable to requested type
        /// </summary>
        [Test]
        public void ValidateThatInvalidCastExceptionIsThrownIfRequestedDataTypeIsNotAssignableFromPropertyValue()
        {
            Person person = new Person() { Name = "Luffy", Age = 16 };
            Assert.Throws<InvalidCastException>(() => { person.GetPropertyValue<int>("Name"); });
        }

        /// <summary>
        /// Validate that property value can be set on an object
        /// </summary>
        [Test]
        public void ValidateThatCanSetPropertyValue()
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
        public void ValidateThatSetPropertyValueShouldThrowExcpetionIfSpecifiedTypeIsIncompatibleWithActualType()
        {

            Person person = new Person()
            {
                Age = 16
            };

            Assert.Throws<Exception>(() => person.SetPropertyValue<string>("Age", "16"));
        }



        [Test]
        public void ValidateThatGetRequiredImportsForTypeCanIdentifyImportsAndReferencesCorrectly()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"#r \"{Path.GetFileName(this.GetType().Assembly.Location)}\"");
            sb.AppendLine($"#r \"{Path.GetFileName(typeof(Person).Assembly.Location)}\"");
            sb.AppendLine("using System.Console;");           
            sb.AppendLine($"using {typeof(Person).Namespace};");

            string imports = typeof(Person).GetRequiredImportsForType(new List<Assembly>() { this.GetType().Assembly }, new List<string>() { "System.Console" });

            Assert.AreEqual(sb.ToString(), imports);
        }

        [Test]
        public void ValidateThatGetRequiredImportsForTypeCanIdentifyImportsAndReferencesCorrectlyWithGenericTypes()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"#r \"{Path.GetFileName(this.GetType().Assembly.Location)}\"");
            sb.AppendLine($"#r \"{Path.GetFileName(typeof(Person).Assembly.Location)}\"");
            sb.AppendLine("using System.Console;");
            sb.AppendLine($"using {typeof(List<>).Namespace};");
            sb.AppendLine($"using {typeof(Person).Namespace};");
       
            string imports = typeof(List<Person>).GetRequiredImportsForType(new List<Assembly>() { this.GetType().Assembly }, new List<string>(){ "System.Console" });
           
            Assert.AreEqual(sb.ToString(), imports);
        }
    }
}
