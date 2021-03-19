using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Roslyn.Utilities;

namespace Pixel.Scripting.Common.CSharp
{
    public class ProjectReferences
    {
        private static readonly Lazy<(string assemblyPath, string docPath)> _referenceAssembliesPath =
            new Lazy<(string, string)>(GetReferenceAssembliesPath);

        private static readonly Lazy<ProjectReferences> _desktopDefault = new Lazy<ProjectReferences>(() =>
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

            //var objectAssemblyPath = typeof(object).Assembly.Location;
            //var mscorlibPath = Path.Combine(Path.GetDirectoryName(objectAssemblyPath), "mscorlib.dll");
            //if (File.Exists(mscorlibPath))
            //{
            //    result = result.With(assemblyPathReferences: new[] { mscorlibPath });
            //}

            //var facadeAssemblies = TryGetFacadeAssemblies(_referenceAssembliesPath.Value.assemblyPath);
            //if (facadeAssemblies != null)
            //{
            //    result = result.With(assemblyPathReferences: facadeAssemblies);
            //}
            //else
            //{
            //    var systemRuntimePath = Path.Combine(Path.GetDirectoryName(objectAssemblyPath), "System.Runtime.dll");
            //    if (File.Exists(systemRuntimePath))
            //    {
            //        result = result.With(assemblyPathReferences: new[] { systemRuntimePath });
            //    }
            //}

            return result;
        });

        private static readonly Lazy<ProjectReferences> _desktopRefsDefault = new Lazy<ProjectReferences>(() =>
        {
            List<MetadataReference> metaDataReferences = new List<MetadataReference>();
            foreach (var file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "refs"), "*.dll"))
            {
                metaDataReferences.Add(MetadataReference.CreateFromFile(file));
            }

            var result = Empty.With(metaDataReferences);
            return result;
        });

        public static ProjectReferences Empty { get; } = new ProjectReferences(
            ImmutableArray<MetadataReference>.Empty,
            ImmutableDictionary<string, string>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase),
            ImmutableArray<string>.Empty);

        /// <summary>
        /// Returns desired defaults for .Net Core Runtime (Use Runtime for scripting)
        /// </summary>
        public static ProjectReferences DesktopDefault => _desktopDefault.Value;


        /// <summary>
        /// Returns desired defaults for .Net Core Refs (Refs are used for compilation i.e. use it with CodeWorkSpace and ScriptWorkSpace)
        /// </summary>
        public static ProjectReferences DesktopRefsDefault => _desktopRefsDefault.Value;

        /// <summary>
        /// Returns namespace-only (no assemblies) defaults that fit all frameworks.
        /// </summary>
        public static ProjectReferences NamespaceDefault { get; } = Empty.With(imports: new[]{
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

        public static (string assemblyPath, string docPath) ReferenceAssembliesPath => _referenceAssembliesPath.Value;

        public ProjectReferences With(IEnumerable<MetadataReference> references = null, IEnumerable<string> imports = null,
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

            return new ProjectReferences(
                _references.AddRange(references ?? Enumerable.Empty<MetadataReference>()),
                referenceLocations,
                importsArray);
        }

        private ProjectReferences(
            ImmutableArray<MetadataReference> references,
            ImmutableDictionary<string, string> referenceLocations,
            ImmutableArray<string> imports)
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

        private static (string assemblyPath, string docPath) GetReferenceAssembliesPath()
        {
            string assemblyPath = null;
            string docPath = null;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                RuntimeInformation.FrameworkDescription.Contains(".NET Core"))
            {
                // all NuGet
                return (assemblyPath, docPath);
            }

            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");

            if (string.IsNullOrEmpty(programFiles))
            {
                programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            }

            if (string.IsNullOrEmpty(programFiles))
            {
                return (assemblyPath, docPath);
            }

            var path = Path.Combine(programFiles, @"Reference Assemblies\Microsoft\Framework\.NETFramework");
            if (Directory.Exists(path))
            {
                assemblyPath = IOUtilities.PerformIO(() => Directory.GetDirectories(path), Array.Empty<string>())
                    .Select(x => new { path = x, version = GetFxVersionFromPath(x) })
                    .OrderByDescending(x => x.version)
                    .FirstOrDefault(x => File.Exists(Path.Combine(x.path, "System.dll")))?.path;

                if (assemblyPath == null || !File.Exists(Path.Combine(assemblyPath, "System.xml")))
                {
                    docPath = GetReferenceDocumentationPath(path);
                }
            }

            return (assemblyPath, docPath);
        }

        private static string GetReferenceDocumentationPath(string path)
        {
            string docPath = null;

            var docPathTemp = Path.Combine(path, "V4.X");
            if (File.Exists(Path.Combine(docPathTemp, "System.xml")))
            {
                docPath = docPathTemp;
            }
            else
            {
                var localeDirectory = IOUtilities.PerformIO(() => Directory.GetDirectories(docPathTemp),
                    Array.Empty<string>()).FirstOrDefault();
                if (localeDirectory != null && File.Exists(Path.Combine(localeDirectory, "System.xml")))
                {
                    docPath = localeDirectory;
                }
            }

            return docPath;
        }

        private static Version GetFxVersionFromPath(string path)
        {
            var name = Path.GetFileName(path);
            if (name?.StartsWith("v", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (Version.TryParse(name.Substring(1), out var version))
                {
                    return version;
                }
            }

            return new Version(0, 0);
        }

        private static IEnumerable<string> TryGetFacadeAssemblies(string referenceAssembliesPath)
        {
            if (referenceAssembliesPath != null)
            {
                var facadesPath = Path.Combine(referenceAssembliesPath, "Facades");
                if (Directory.Exists(facadesPath))
                {
                    return Directory.EnumerateFiles(facadesPath, "*.dll");
                }
            }

            return null;
        }
    }
}