using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.TypeLookup
{
    public class TypeLookupResponse
    {
        public string Type { get; set; }

        public string Documentation { get; set; }

        public DocumentationComment StructuredDocumentation { get; set; }

        public IEnumerable<SymbolDisplayPart> SymbolDisplayParts { get; set; }

        public Glyph Glyph { get; set; }
    }   
}
