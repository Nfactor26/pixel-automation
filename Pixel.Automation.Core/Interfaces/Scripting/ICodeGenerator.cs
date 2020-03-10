using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces.Scripting
{
    public interface  ICodeGenerator
    {
        IClassGenerator CreateClassGenerator(string className, string nameSpace, IEnumerable<string> imports);    
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
