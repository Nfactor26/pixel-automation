using System.Collections.Generic;

namespace Pixel.Automation.Scripting.Components.Tests
{

    public class Address
    {
     
        public string City { get; set; }

        public string Country { get; set; }
       
    }

    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public Address Address { get; set; }

        public List<Person> Friends { get; } = new List<Person>();
    }
}
