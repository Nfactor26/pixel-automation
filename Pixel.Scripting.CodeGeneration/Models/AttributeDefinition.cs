using System;
using System.Collections.Generic;

namespace Pixel.Scripting.CodeGeneration
{
    public class AttributeDefinition
    {
        public string TargetProperty { get; set; }
        public Type AttributeType { get; set; }

        public IEnumerable<KeyValuePair<string, object>> AttributeParameters = new List<KeyValuePair<string, object>>();
    }
}
