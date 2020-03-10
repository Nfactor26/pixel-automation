using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace Pixel.Automation.Designer.ViewModels
{
    public class KnownTypeProvider : ITypeProvider
    { 
        ISerializer serializer;
        public Dictionary<string, List<Type>> KnownTypes { get; } = new Dictionary<string, List<Type>>();

        #region constructor

        public KnownTypeProvider(ISerializer serializer)
        {           
            this.serializer = serializer;
            LoadDefaultTypes();
        }

        #endregion constructor

        #region public methods

        public void LoadDefaultTypes()
        {
            this.KnownTypes.Clear();
            if(!TryLoadTypeFromCache())
            {
                GenerateTypeCache();
            }            
        }
  
        public void RefreshDefaultTypeCache()
        {
            var typeCacheFile = Path.Combine("Components", "TypeCache.bin");
            if (File.Exists(typeCacheFile))
            {
                File.Delete(typeCacheFile);
            }
            LoadDefaultTypes();
        }

        public void LoadTypesFromAssembly(List<string> loadFromAssemblies)
        {

            foreach (var assemblyPath in loadFromAssemblies)
            {
                string assemblyName = Path.GetFileName(assemblyPath);
                assemblyName = assemblyName.Substring(0, assemblyName.LastIndexOf('_'));
                             
                if (KnownTypes.ContainsKey(assemblyName))
                    KnownTypes.Remove(assemblyName);
            }

            //GC.Collect();

            foreach (var assemblyPath in loadFromAssemblies)
            {
                

                List<Type> customTypes = new List<Type>();
                var assembly = Assembly.LoadFrom(assemblyPath);
                foreach (Type t in assembly.DefinedTypes)
                {
                    if (t.IsPublic && !t.IsAbstract && (t.IsSubclassOf(typeof(Component)) || t.GetInterface("IEntityProcessor") != null) )
                    {
                        customTypes.Add(t);
                    }

                }

                if (customTypes.Count()>0)
                {
                    string assemblyName = Path.GetFileName(assemblyPath);
                    assemblyName = assemblyName.Substring(0, assemblyName.LastIndexOf('_'));
                    KnownTypes.Add(assemblyName, customTypes);
                }
            }          
           
           
        }

        public void LoadTypesFromAssembly(Assembly assembly)
        {
            List<Type> customTypes = new List<Type>();
            foreach (Type t in assembly.DefinedTypes)
            {
                if (t.IsPublic && !t.IsAbstract)
                {
                    customTypes.Add(t);
                }

            }
            KnownTypes.Add(assembly.FullName, customTypes);
        }

        public void ClearCustomTypes()
        {
            var defaultTypes = KnownTypes["Default"];
            KnownTypes.Clear();
            KnownTypes.Add("Default",defaultTypes);
        }

        public List<Type> GetAllTypes()
        {
            List<Type> allKnownComponents = new List<Type>();
            foreach (var typeCollection in KnownTypes)
            {
                allKnownComponents.AddRange(typeCollection.Value);
            }
            return allKnownComponents;
        }

        #endregion public methods

        #region private methods

        private void GenerateTypeCache()
        {
            List<Type> defaultTypes = new List<Type>();

            //foreach (var item in IoC.GetAll<IComponent>())
            //    defaultTypes.Add(item.GetType());

            //foreach (var item in IoC.GetAll<IApplication>())
            //    defaultTypes.Add(item.GetType());

            //foreach(var assembly in Directory.EnumerateFiles("Components"))
            //{
            //    try
            //    {
            //        if (assembly.Contains("Pixel"))
            //        {
            //            Assembly pixelAssembly = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory,assembly));
            //            var definedTypes = pixelAssembly.DefinedTypes.Where(t => t.IsPublic && !t.IsAbstract);
            //            foreach (var definedType in definedTypes)
            //                defaultTypes.Add(definedType);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //    }

            //}

            foreach (var assembly in Directory.EnumerateFiles(".","Pixel.*.dll"))
            {
                try
                {
                    switch(assembly)
                    {
                        case ".\\Pixel.Automation.Input.Devices.dll":
                        case ".\\Pixel.Automation.Web.Selenium.Components.dll":
                        case ".\\Pixel.Automation.Window.Management.dll":
                        case ".\\Pixel.Automation.Core.Components.dll":
                        case ".\\Pixel.Automation.Core.Utilities.dll":
                        case ".\\Pixel.Automation.RunTime.dll":
                        case ".\\Pixel.Automation.Scripting.Components.dll":
                        case ".\\Pixel.Automation.UIA.Components.dll":
                            Assembly pixelAssembly = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, assembly));
                            var definedTypes = pixelAssembly.DefinedTypes.Where(t => t.IsPublic && !t.IsAbstract);
                            foreach (var definedType in definedTypes)
                                defaultTypes.Add(definedType);
                            break;

                    }
                   
                }
                catch (Exception ex)
                {
                }

            }

            KnownTypes.Add("Default", defaultTypes);

            var typeCacheFile = Path.Combine("Components", "TypeCache.bin");
            serializer.Serialize<List<Type>>(typeCacheFile, defaultTypes);
        }

        private bool TryLoadTypeFromCache()
        {
            var typeCacheFile = Path.Combine("Components", "TypeCache.bin");
            if (File.Exists(typeCacheFile))
            {
                var cachedTypes = serializer.Deserialize<List<Type>>(typeCacheFile);
                this.KnownTypes.Add("Default", cachedTypes);
                return true;
            }
            return false;
        }

        #endregion private methods
    }
}
