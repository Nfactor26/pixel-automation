using Microsoft.CodeAnalysis;
using Roslyn.Utilities;
using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Pixel.Scripting.Reference.Manager;

/// <summary>
/// Runtime references for Projects
/// </summary>
public class AssemblyReferences
{

    private readonly ImmutableArray<MetadataReference> _references;

    private readonly ImmutableDictionary<string, string> _referenceLocations;

    public static AssemblyReferences Empty { get; } = new AssemblyReferences(ImmutableArray<MetadataReference>.Empty,
            ImmutableDictionary<string, string>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase),
            ImmutableArray<string>.Empty);


    private static readonly Lazy<AssemblyReferences> defaultReferences = new Lazy<AssemblyReferences>(() =>
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
    public static AssemblyReferences DefaultReferences => defaultReferences.Value;
          
    /// <summary>
    /// Returns namespace-only (no assemblies) defaults that fit all frameworks.
    /// </summary>
    public static AssemblyReferences DefaultNamespaces { get; } = Empty.With(imports: new[]{
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

    public ImmutableArray<string> Imports { get; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="references"></param>
    /// <param name="referenceLocations"></param>
    /// <param name="imports"></param>
    private AssemblyReferences(ImmutableArray<MetadataReference> references, ImmutableDictionary<string, string> referenceLocations, ImmutableArray<string> imports)
    {
        _references = references;
        _referenceLocations = referenceLocations;
        Imports = imports;
    }   

    public AssemblyReferences With(IEnumerable<MetadataReference> references = null, IEnumerable<string> imports = null,
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

        return new AssemblyReferences(_references.AddRange(references ?? Enumerable.Empty<MetadataReference>()), referenceLocations, importsArray);
    }


    public ImmutableArray<MetadataReference> GetReferences(Func<string, DocumentationProvider> documentationProviderFactory = null) =>
        Enumerable.Concat(_references, Enumerable.Select(_referenceLocations, c => MetadataReference.CreateFromFile(c.Key, documentation: documentationProviderFactory?.Invoke(c.Key))))
            .ToImmutableArray();

   
}