using Pixel.Scripting.Editor.Core.Models.TypeLookup;
using System.Linq;
using System.Windows.Controls;

namespace Pixel.Scripting.Script.Editor.Model
{
    public class TypeDescription
    {
        public bool HasDisplayParts { get; }

        public TextBlock TypeDisplay { get; }

        public TypeLookupResponse TypeDetails { get; }

        public bool HasDocumentation
        {
            get => !string.IsNullOrEmpty(TypeDetails.Documentation);
        }

        public TypeDescription(TypeLookupResponse typeLookupResponse)
        {
            if(typeLookupResponse.SymbolDisplayParts !=  null)
            {
                this.HasDisplayParts = typeLookupResponse.SymbolDisplayParts != null && typeLookupResponse.SymbolDisplayParts.Any();
                if (HasDisplayParts)
                {
                    this.TypeDisplay = typeLookupResponse.SymbolDisplayParts.ToTextBlock();
                }
            }
        
            this.TypeDetails = typeLookupResponse;
        }
       
    }
}
