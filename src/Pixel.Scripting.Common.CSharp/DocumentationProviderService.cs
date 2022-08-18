using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.IO;

namespace Pixel.Scripting.Common.CSharp
{
    public sealed class DocumentationProviderService
    {
        private readonly ConcurrentDictionary<string, DocumentationProvider> _assemblyPathToDocumentationProviderMap = new ();

        public DocumentationProvider GetDocumentationProvider(string location)
        {
            var finalPath = Path.ChangeExtension(location, "xml");

            return _assemblyPathToDocumentationProviderMap.GetOrAdd(location, _ =>
                {                   
                    return finalPath == null ? null : XmlDocumentationProvider.CreateFromFile(finalPath);
                });
        }   
    }

}