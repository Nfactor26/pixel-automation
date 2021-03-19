using System;
using System.Collections.Generic;

namespace Pixel.Scripting.CodeGeneration
{
    public class PropertyDefinition
    {
        public string PropertyName { get; set; }

        public Type PropertyType { get; set; }

        public string DefaultValue { get; set; }

        public IEnumerable<Modifiers> PropertyModifiers { get; set; } = new List<Modifiers> { Modifiers.Public };

    }
}
