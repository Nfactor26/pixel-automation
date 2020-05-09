using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Scripting.Engine.CSharp.Tests
{
    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public List<Person> Friends { get; } = new List<Person>();
    }
}
