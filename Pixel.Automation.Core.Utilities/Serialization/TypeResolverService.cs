using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace Pixel.Automation.Core.Utilities
{
    public class TypeResolverService : DataContractResolver
    {
        //private static TypeResolverService resolver;
        //public static TypeResolverService Resolver
        //{
        //    get
        //    {
        //        if (resolver == null)
        //            resolver = new TypeResolverService();
        //        return resolver;
        //    }
        //}

        List<Assembly> knownAssemblies;


        public TypeResolverService()
        {
            this.knownAssemblies = new List<Assembly>();
        }

        public void AddAssembly(Assembly assembly)
        {
            if(!this.knownAssemblies.Contains(assembly))
                this.knownAssemblies.Add(assembly);
        }

        public void RemoveAssembly(Assembly assembly)
        {
            if (this.knownAssemblies.Contains(assembly))
                this.knownAssemblies.Remove(assembly);
        }

        public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            //if (knownTypeResolver.TryResolveType(dataContractType, declaredType, null, out typeName, out typeNamespace))
            //{
            //    return true;
            //}
            //else
            //{
                XmlDictionary dictionary = new XmlDictionary();
                typeName = dictionary.Add(dataContractType.FullName);
                typeNamespace = dictionary.Add(dataContractType.Assembly.FullName);
                return true;
            //}
           
        }

        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            Type targetType = null;

            while (targetType == null)
            {
                foreach (var assembly in this.knownAssemblies)
                {
                    try
                    {
                        targetType = assembly.GetType(typeName, false);
                    }
                    catch
                    {

                    }
                }
            }
          
            if (targetType != null)
                return targetType;
            return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null) ?? Type.GetType(typeName + ", " + typeNamespace);
        }
    }
}
