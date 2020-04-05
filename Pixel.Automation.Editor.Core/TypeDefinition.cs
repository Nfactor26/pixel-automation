using Caliburn.Micro;
using System;
using System.Linq;
using System.Reflection;

namespace Pixel.Automation.Editor.Core
{
    public class TypeDefinition : PropertyChangedBase
    {
        public string AssemblyName { get; private set; }

        public string NameSpace { get; private set; }

        public string DisplayName { get; private set; }

        public string FullName { get; private set; }

        public bool IsGenericType { get; set; }

        public string FullNameWithoutOpenType
        {
            get
            {
                return this.FullName.Split(new char[] { '<' })[0] ?? this.FullName;
            }
        }   


        Type actualType;
        public Type ActualType
        {
            get => this.actualType;
            set
            {
                this.actualType = value;
                //if (value != null)
                //    PopuplatePropertiesFromType(value);               
                //NotifyOfPropertyChange(() => AssemblyName);
                //NotifyOfPropertyChange(() => NameSpace);
                //NotifyOfPropertyChange(() => DisplayName);
                //NotifyOfPropertyChange(() => GenericParameters);
                NotifyOfPropertyChange(() => ActualType);               
            }
        }

        public TypeDefinition(Type actualType)
        {           
            this.actualType = actualType;
            PopuplatePropertiesFromType(actualType);
        }

        public TypeDefinition(string assemblyName, string typeNameSpace, Type type) : this(type)
        {
            this.AssemblyName = assemblyName;
            this.NameSpace = typeNameSpace;
        }

        private void PopuplatePropertiesFromType(Type type)
        {
            this.AssemblyName = GetFriendlyNameForAssembly(type.Assembly) ?? string.Empty;
            this.NameSpace = type.Namespace ?? string.Empty;         
            this.IsGenericType = type.IsGenericType;
            this.DisplayName = GetDisplayName(type);
            this.FullName = $"{this.NameSpace}.{this.DisplayName}";           
        }

        private string GetFriendlyNameForAssembly(Assembly assembly)
        {
            var assemblyName = assembly.GetName();
            return $"{assemblyName.Name} [{assemblyName.Version}]";
        }


        public string GetDisplayName(Type type)
        {
            switch (type.IsGenericType)
            {
                case true:                   
                    return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetDisplayName(x)).ToArray()) + ">";
                case false:
                    return type.Name;
            }           
        }

        //public string GetDisplayNameWithoutOpenTypes(Type type)
        //{
        //    switch (type.IsGenericType)
        //    {
        //        case true:
        //            if (type.ContainsGenericParameters)
        //                return type.Name.Split('`')[0];
        //            else
        //                return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetDisplayName(x)).ToArray()) + ">";
        //        case false:
        //            return type.Name;
        //    }
        //    return type.Name;
        //}


        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
