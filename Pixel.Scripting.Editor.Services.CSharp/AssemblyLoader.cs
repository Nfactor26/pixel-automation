using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Pixel.Script.Editor.Services.CSharp
{
    internal class AssemblyLoader : IAssemblyLoader, IAnalyzerAssemblyLoader
    {
        private static readonly ConcurrentDictionary<string, Assembly> AssemblyCache = new ConcurrentDictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);      
        public AssemblyLoader()
        {
         
        }

        public void AddDependencyLocation(string fullPath)
        {
            LoadFrom(fullPath);
        }

        public Assembly Load(AssemblyName name)
        {
            Assembly result = null;
            try
            {
                result = Assembly.Load(name);               
            }
            catch (Exception ex)
            {
             
            }

            return result;
        }

        public IReadOnlyList<Assembly> LoadAllFrom(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) return Array.Empty<Assembly>();

            try
            {
                var assemblies = new List<Assembly>();

                foreach (var filePath in Directory.EnumerateFiles(folderPath, "*.dll"))
                {
                    var assembly = LoadFrom(filePath);
                    if (assembly != null)
                    {
                        assemblies.Add(assembly);
                    }
                }

                return assemblies;
            }
            catch (Exception ex)
            {             
                return Array.Empty<Assembly>();
            }
        }

        public Assembly LoadFrom(string assemblyPath, bool dontLockAssemblyOnDisk = false)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath)) return null;

            if (!AssemblyCache.TryGetValue(assemblyPath, out var assembly))
            {
                try
                {
                    if (dontLockAssemblyOnDisk)
                    {
                        var bytes = File.ReadAllBytes(assemblyPath);
                        assembly = Assembly.Load(bytes);
                    }
                    else
                    {
                        assembly = Assembly.LoadFrom(assemblyPath);
                    }
                }
                catch (Exception ex)
                {                 
                    return assembly;
                }

                AssemblyCache.AddOrUpdate(assemblyPath, assembly, (k, v) => assembly);
            }
         
            return assembly;
        }

        public Assembly LoadFromPath(string fullPath)
        {
            return LoadFrom(fullPath);
        }
    }
}
