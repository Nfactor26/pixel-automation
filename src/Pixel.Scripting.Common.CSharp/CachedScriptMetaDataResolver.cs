using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Pixel.Scripting.Common.CSharp
{
    public class CachedScriptMetadataResolver : MetadataReferenceResolver
    {
        private readonly ScriptMetadataResolver _inner;
        private readonly ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>> _cache;
        private readonly List<string> _whiteListedReferences = new List<string>();

        public CachedScriptMetadataResolver(ScriptMetadataResolver scriptMetaDataResolver, bool useCache = false)
        {
            _inner = scriptMetaDataResolver;
            if (useCache)
            {
                _cache = new ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>>();
            }
        }

        public override bool Equals(object other) => _inner.Equals(other);

        public override int GetHashCode() => _inner.GetHashCode();

        public override bool ResolveMissingAssemblies => _inner.ResolveMissingAssemblies;

        /// <summary>
        /// Tries to resolve dependencies for primary references. This goes crazy and loads like few hunder assemblies which takes like additional 50-60 mb
        /// of memory.We have a whitelist now which controls what references can be resolved here. For editors, we don't add anything to whitelist. 
        /// For script engine however, this will be set from white list defined in app settings file when script engine factory creates the ScirptMetaDataResolver.
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="referenceIdentity"></param>
        /// <returns></returns>
        public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
        {
            if (!_whiteListedReferences.Contains(referenceIdentity.Name))
            {
                return null;
            }

            if (_cache == null)
            {
                return _inner.ResolveMissingAssembly(definition, referenceIdentity);
            }

            return _cache.GetOrAdd(referenceIdentity.ToString(),
                _ => ImmutableArray.Create(_inner.ResolveMissingAssembly(definition, referenceIdentity))).FirstOrDefault();
          
        }

        /// <summary>
        /// This method resolves the primary assemblies which are added to editors and script engines by host application.
        /// This will also resolve #r references for script editors and scripts at runtime.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="baseFilePath"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {            
            if (_cache == null)
            {
                return _inner.ResolveReference(reference, baseFilePath, properties);
            }

            if (!_cache.TryGetValue(reference, out var result))
            {
                result = _inner.ResolveReference(reference, baseFilePath, properties);
                if (!result.IsDefaultOrEmpty)
                {
                    _cache.TryAdd(reference, result);
                }
            }

            return result;
        }

        public MetadataReferenceResolver WithWhiteListedReference(string assemblyName)
        {
            if(!_whiteListedReferences.Contains(assemblyName))
            {
                _whiteListedReferences.Add(assemblyName);
            }
            return this;
        }
    }  
}
