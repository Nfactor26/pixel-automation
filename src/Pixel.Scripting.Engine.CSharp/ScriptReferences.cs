using Microsoft.CodeAnalysis;
using Roslyn.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Scripting.Engine.CSharp
{
    /// <summary>
    /// Runtime references for Script Engine
    /// </summary>
    public class ScriptReferences
    {     
      
        public static ScriptReferences Empty { get; } = new ScriptReferences(ImmutableArray<MetadataReference>.Empty,
            ImmutableDictionary<string, string>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase),
            ImmutableArray<string>.Empty);


        private static readonly Lazy<ScriptReferences> _desktopDefault = new Lazy<ScriptReferences>(() =>
        {
            var result = Empty.With(typeNamespaceImports: new[]
            {
                typeof(object),
                typeof(Thread),
                typeof(Task),
                typeof(List<>),
                typeof(Regex),
                typeof(StringBuilder),
                typeof(Uri),
                typeof(Enumerable),
                typeof(IEnumerable),
                typeof(Path),
                typeof(Assembly)
            }, assemblyReferences: new[]
            {
                typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly
            });
            return result;
        });

        /// <summary>
        /// Returns desired defaults for .Net Core Runtime (Use Runtime for scripting)
        /// </summary>
        public static ScriptReferences DesktopDefault => _desktopDefault.Value;
              
        /// <summary>
        /// Returns namespace-only (no assemblies) defaults that fit all frameworks.
        /// </summary>
        public static ScriptReferences NamespaceDefault { get; } = Empty.With(imports: new[]{
            "System",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Collections",
            "System.Collections.Generic",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Linq",
            "System.IO",
            "System.Reflection",
        });
     
        public ScriptReferences With(IEnumerable<MetadataReference> references = null, IEnumerable<string> imports = null,
            IEnumerable<Assembly> assemblyReferences = null, IEnumerable<string> assemblyPathReferences = null, IEnumerable<Type> typeNamespaceImports = null)
        {
            var referenceLocations = _referenceLocations;
            var importsArray = Imports.AddRange(imports ?? Enumerable.Empty<string>());

            var locations =
                assemblyReferences?.Select(c => c.Location).Concat(
                assemblyPathReferences ?? Enumerable.Empty<string>()) ?? Enumerable.Empty<string>();

            foreach (var location in locations)
            {
                referenceLocations = referenceLocations.SetItem(location, string.Empty);
            }

            foreach (var type in typeNamespaceImports ?? Enumerable.Empty<Type>())
            {
                importsArray = importsArray.Add(type.Namespace);
                var location = type.Assembly.Location;
                referenceLocations = referenceLocations.SetItem(location, string.Empty);
            }  

            return new ScriptReferences(_references.AddRange(references ?? Enumerable.Empty<MetadataReference>()), referenceLocations, importsArray);
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="references"></param>
        /// <param name="referenceLocations"></param>
        /// <param name="imports"></param>
        private ScriptReferences(ImmutableArray<MetadataReference> references, ImmutableDictionary<string, string> referenceLocations, ImmutableArray<string> imports)
        {
            _references = references;
            _referenceLocations = referenceLocations;
            Imports = imports;
        }

        private readonly ImmutableArray<MetadataReference> _references;

        private readonly ImmutableDictionary<string, string> _referenceLocations;

        public ImmutableArray<string> Imports { get; }

        public ImmutableArray<MetadataReference> GetReferences(Func<string, DocumentationProvider> documentationProviderFactory = null) =>
            Enumerable.Concat(_references, Enumerable.Select(_referenceLocations, c => MetadataReference.CreateFromFile(c.Key, documentation: documentationProviderFactory?.Invoke(c.Key))))
                .ToImmutableArray();

       
    }
}