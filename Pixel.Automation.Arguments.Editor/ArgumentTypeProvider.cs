using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
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
        private List<TypeDefinition> commonTypes;
        private List<TypeDefinition> allKnownTypes;
        private List<TypeDefinition> definedTypes;


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
                    }
                    catch (Exception)
                    {                       
                        continue;
                    }
                }
            }
            return this;
        }

        public IArgumentTypeProvider WithDataModelAssembly(Assembly assembly)
        {
            this.definedTypes = new List<TypeDefinition>();
            foreach(var type in PopulateAvailableTypesInAssembly(assembly).Skip(1))
            {
              this.definedTypes.Add(new TypeDefinition("Application.Process.Data","DataModel",type));
            }
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
            return definedTypes ?? new List<TypeDefinition>();
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
        }

        private IEnumerable<Type> PopulateAvailableTypesInAssembly(Assembly assembly)
        {
            if (assembly.IsDynamic)
                yield break;          
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
