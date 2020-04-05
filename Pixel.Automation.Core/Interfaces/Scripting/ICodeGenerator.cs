using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces.Scripting
{
    public interface  ICodeGenerator
    {
        /// <summary>
        /// Create a IClassGenerator which can be used to  build a class e.g. add properties to it.
        /// </summary>
        /// <param name="className"></param>
        /// <param name="nameSpace"></param>
        /// <param name="imports"></param>
        /// <returns></returns>
        IClassGenerator CreateClassGenerator(string className, string nameSpace, IEnumerable<string> imports);

        /// <summary>
        /// Generate Class with Properties from a given type.    
        /// </summary>
        /// <param name="targetType">TargetType whose properties needs to be mirrored</param>     
        /// <returns></returns>
        string GenerateClassForType(Type targetType, string nameSpace, IEnumerable<string> imports);
    }

    public interface IClassGenerator
    {
        IClassGenerator AddProperty(string propertyName, Type propertyType, string defaultValue = "");

        IClassGenerator AddAttribute(string targetProperty, Type attributeType, IEnumerable<KeyValuePair<string, object>> attributeArguments);

        IClassGenerator SetBaseClass(Type baseClassType);

        string GetGeneratedCode();

        bool HasErrors(out string errorMessage);

    }

}
