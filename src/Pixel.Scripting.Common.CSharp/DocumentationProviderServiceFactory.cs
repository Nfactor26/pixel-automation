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

            return _assemblyPathToDocumentationProviderMap.GetOrAdd(location,
                _ =>
                {
                    if (!File.Exists(finalPath))
                    {
                        finalPath = GetFilePath(ProjectReferences.ReferenceAssembliesPath.docPath, finalPath) ??
                                    GetFilePath(ProjectReferences.ReferenceAssembliesPath.assemblyPath, finalPath);
                    }

                    return finalPath == null ? null : XmlDocumentationProvider.CreateFromFile(finalPath);
                });
        }

        private static string GetFilePath(string path, string location)
        {
            if (path != null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var referenceLocation = Path.Combine(path, Path.GetFileName(location));
                if (File.Exists(referenceLocation))
                {
                    return referenceLocation;
                }
            }

            return null;
        }
    }

}