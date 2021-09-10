using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using System.Collections.Concurrent;
using System.Composition;
using System.IO;

namespace Pixel.Scripting.Common.CSharp
{
    [ExportWorkspaceServiceFactory(typeof(IDocumentationProviderService), ServiceLayer.Host), Shared]
    internal sealed class DocumentationProviderServiceFactory : IWorkspaceServiceFactory
    {
        private readonly IDocumentationProviderService _service;

        [ImportingConstructor]
        public DocumentationProviderServiceFactory(IDocumentationProviderService service) => _service = service;

        public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices) => _service;
    }

    [Export(typeof(IDocumentationProviderService)), Shared]
    internal sealed class DocumentationProviderService : IDocumentationProviderService
    {
        private readonly ConcurrentDictionary<string, DocumentationProvider> _assemblyPathToDocumentationProviderMap
            = new ConcurrentDictionary<string, DocumentationProvider>();

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