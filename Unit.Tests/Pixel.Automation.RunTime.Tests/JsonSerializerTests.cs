using NUnit.Framework;
using Pixel.Automation.Test.Helpers;
using System.IO;
using System.Text;
using Pixel.Automation.RunTime.Serialization;

namespace Pixel.Automation.Core.RunTime.Tests
{
    public class JsonSerializerTests
    {        

        [Test]
        public void ValidateThatJsonSerializerCanSerializerAndDeserializeDataToAndFromFile()
        {
            var serializer = new JsonSerializer();

            var person = new Person()
            {
                Name = "Sasuke Uchiha",
                Age = 26,
                Address = new Address() { Country = "Land of Fire", City = "Hidden Leaf" },
                Friends = new System.Collections.Generic.List<Person>()
                {
                    new Person()
                    {
                          Name = "Naruto Shippuden",
                          Age = 26,
                          Address = new Address() { Country = "Land of Fire", City = "Hidden Leaf"},
                    }
                }
            };

            serializer.Serialize<Person>("Person.json", person);

            Assert.IsTrue(File.Exists("Person.json"));

            var result = serializer.Deserialize<Person>("Person.json");

            Assert.IsNotNull(result);

            Assert.AreEqual(person, result);
            Assert.AreEqual(1, result.Friends.Count);
        }

        [Test]
        public void ValidateThatJsonSerializerCanDeserializerStringContent()
        {
            var serializer = new JsonSerializer();
            var jsonString = new StringBuilder();
            jsonString.AppendLine("{");
            jsonString.AppendLine("\"Name\" : \"Sasuke Uchiha\",");
            jsonString.AppendLine("\"Age\" : 26");
            jsonString.AppendLine("}");

            var person = serializer.DeserializeContent<Person>(jsonString.ToString());
            Assert.IsNotNull(person);
            Assert.AreEqual("Sasuke Uchiha", person.Name);
            Assert.AreEqual(26, person.Age);
        }
    }
}