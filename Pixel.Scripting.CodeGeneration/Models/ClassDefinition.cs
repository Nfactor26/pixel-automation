using System.Collections.Generic;

namespace Pixel.Scripting.CodeGeneration
{
    public class ClassDefinition
    {
        public string ClassName { get; set; }

        public string NameSpace { get; set; }

        public IEnumerable<string> Imports { get; set; }

        public IEnumerable<Modifiers> ClassModifiers { get; set; } = new List<Modifiers> { Modifiers.Public };
    }
}
