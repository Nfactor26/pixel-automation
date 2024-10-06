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

            Assert.That(File.Exists("Person.json"));

            var result = serializer.Deserialize<Person>("Person.json");

            Assert.That(result is not null);

            Assert.That(result, Is.EqualTo(person));
            Assert.That(result.Friends.Count, Is.EqualTo(1));
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
            Assert.That(person is not null);
            Assert.That(person.Name, Is.EqualTo("Sasuke Uchiha"));
            Assert.That(person.Age, Is.EqualTo(26));
        }
    }
}