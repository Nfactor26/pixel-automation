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
        private ScriptMetadataResolver resolver;
        private readonly ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>> cache;
        private readonly List<string> whiteListedReferences = new List<string>();

        public ImmutableArray<string> SearchPaths => resolver.SearchPaths;
        
        public string BaseDirectory => resolver.BaseDirectory;

        public CachedScriptMetadataResolver(ScriptMetadataResolver scriptMetaDataResolver, bool useCache = false)
        {
            resolver = scriptMetaDataResolver;
            if (useCache)
            {
                cache = new ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>>();
            }
        }

        public override bool Equals(object other) => resolver.Equals(other);

        public override int GetHashCode() => resolver.GetHashCode();

        public override bool ResolveMissingAssemblies => resolver.ResolveMissingAssemblies;

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
            if (!whiteListedReferences.Contains(referenceIdentity.Name))
            {
                return null;
            }

            if (cache == null)
            {
                return resolver.ResolveMissingAssembly(definition, referenceIdentity);
            }

            return cache.GetOrAdd(referenceIdentity.ToString(),
                _ => ImmutableArray.Create(resolver.ResolveMissingAssembly(definition, referenceIdentity))).FirstOrDefault();
          
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
            if (cache == null)
            {
                return resolver.ResolveReference(reference, baseFilePath, properties);
            }

            if (!cache.TryGetValue(reference, out var result))
            {
                result = resolver.ResolveReference(reference, baseFilePath, properties);
                if (!result.IsDefaultOrEmpty)
                {
                    cache.TryAdd(reference, result);
                }
            }

            return result;
        }


        public CachedScriptMetadataResolver WithScriptMetaDataResolver(ScriptMetadataResolver scriptMetaDataResolver)
        {
            this.resolver = scriptMetaDataResolver;
            return this;
        }

        public CachedScriptMetadataResolver WithWhiteListedReference(string assemblyName)
        {
            if(!whiteListedReferences.Contains(assemblyName))
            {
                whiteListedReferences.Add(assemblyName);
            }
            return this;
        }
    }  
}
