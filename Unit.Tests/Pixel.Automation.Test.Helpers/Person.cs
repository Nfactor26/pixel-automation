using System.Collections.Generic;

namespace Pixel.Automation.Test.Helpers
{  

    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public Address Address { get; set; }

        public List<Person> Friends { get; } = new List<Person>();
    }
}
