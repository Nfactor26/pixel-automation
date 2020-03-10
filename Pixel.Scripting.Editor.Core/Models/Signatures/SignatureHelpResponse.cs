using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Models.Signatures
{
    public class SignatureHelpResponse
    {
        public IEnumerable<SignatureHelpItem> Signatures { get; set; }

        public int ActiveSignature { get; set; }
        
        public int ActiveParameter { get; set; }
    }
}