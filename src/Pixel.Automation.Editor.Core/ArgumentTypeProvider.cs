using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pixel.Automation.Arguments.Editor
{
    public class ArgumentTypeProvider : IArgumentTypeProvider
    {
        private readonly ILogger logger = Log.ForContext<ArgumentTypeProvider>();
        private List<TypeDefinition> commonTypes;
        private List<TypeDefinition> allKnownTypes;
        private List<TypeDefinition> dataModelTypes = new List<TypeDefinition>();


        public ArgumentTypeProvider()
        {
           
        }

        public IArgumentTypeProvider WithAdditionalAssemblyPaths(params string[] assemblyPaths)
        {
            foreach(var directory in assemblyPaths)
            {
                foreach(var assemblyPath in Directory.EnumerateFiles(directory,"*.dll",SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFile(assemblyPath);                      
                        PopulateAvailableTypesInAssembly(assembly);
                        logger.Information($"Adding types from assembly {assembly.FullName}");
                    }
                    catch (Exception)
                    {                       
                        continue;
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Populate types defined in data model assembly. Clears any prevoius cached types for older data model assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public IArgumentTypeProvider WithDataModelAssembly(Assembly assembly)
        {
            logger.Information($"Adding types from data model assembly {assembly.FullName}");
            this.dataModelTypes.Clear();
            foreach(var type in PopulateAvailableTypesInAssembly(assembly))
            {
              this.dataModelTypes.Add(new TypeDefinition("DataModel", "Models", type));
            }
            logger.Information($"Data model assembly has {dataModelTypes.Count} defined types");
            return this;
        }

        public IEnumerable<TypeDefinition> GetAllKnownTypes()
        {
            if (allKnownTypes == null)
                InitializeAllKnownTypes();
            return allKnownTypes;
        }

        public IEnumerable<TypeDefinition> GetCommonTypes()
        {
            if (commonTypes == null)
                InitializeCommonTypes();
            return commonTypes;
        }

        public IEnumerable<TypeDefinition> GetCustomDefinedTypes()
        {
            return dataModelTypes;
        }

        private void InitializeCommonTypes()
        {
            commonTypes = new List<TypeDefinition>();
            Type[] mostCommonTypes = { typeof(int), typeof(double), typeof(float), typeof(string),
                    typeof(bool), typeof(object), typeof(IEnumerable<>), typeof(List<>),typeof(Dictionary<,>) };
            foreach (var type in mostCommonTypes)
            {
                commonTypes.Add(new TypeDefinition(type));
            }
            logger.Information($"Initialized commmon types. Total count : {commonTypes.Count}");
        }

        private void InitializeAllKnownTypes()
        {
            allKnownTypes = new List<TypeDefinition>();
            var allAssembliesInAppDomain = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAssembliesInAppDomain)
            {
                CheckIfAssemblyHasAnyIssues(assembly);
                foreach (var type in PopulateAvailableTypesInAssembly(assembly))
                {
                    if (!string.IsNullOrEmpty(type.Namespace))
                        this.allKnownTypes.Add(new TypeDefinition(type));
                }
            }
            logger.Information($"Initialized all known types. Total count : {allKnownTypes.Count}");
        }

        private IEnumerable<Type> PopulateAvailableTypesInAssembly(Assembly assembly)
        {
            if (assembly.IsDynamic)
            {
                logger.Warning($"Assembly : {assembly.FullName} is a dynamic assembly. Skip identify defined types in assembly");
                yield break;

            }
            var publicTypesInAssembly = assembly.GetExportedTypes().Where(t => (t.IsClass) || t.IsValueType || t.IsEnum);
            foreach (var type in publicTypesInAssembly.Distinct())
            {
                yield return type;
            }           
        }


        [Conditional("DEBUG")]
        void CheckIfAssemblyHasAnyIssues(Assembly assembly)
        {
            if (assembly.IsDynamic)
                return;
            if (string.IsNullOrEmpty(assembly.FullName))
                Debug.WriteLine(false, "Assembly name can't be null or empty");
            var groupedTypes = assembly.GetExportedTypes().Where(t => (!t.IsAbstract && t.IsClass) || t.IsValueType || t.IsEnum).GroupBy(t => t.Namespace);
            foreach (var group in groupedTypes)
            {
                if (string.IsNullOrEmpty(group.Key))
                    Debug.WriteLine(false, "Namespace can't be null or empty");
            }

        }
    }
}
