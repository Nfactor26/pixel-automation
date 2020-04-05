using Dawn;
using Pixel.Automation.Core.Interfaces.Scripting;
using System;
using System.Collections.Generic;

namespace Pixel.Scripting.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        public IClassGenerator CreateClassGenerator(string className, string nameSpace, IEnumerable<string> imports)
        {
            Guard.Argument(className).NotNull();
            Guard.Argument(nameSpace).NotNull();
            return new ClassBuilder(className, nameSpace, imports);
        }

        public string GenerateClassForType(Type targetType, string nameSpace, IEnumerable<string> imports)
        {
            IClassGenerator classGenerator = CreateClassGenerator((new TypeSyntaxGenerator().GetDisplayName(targetType)),
                nameSpace, imports);
            foreach(var property in targetType.GetProperties())
            {
                classGenerator.AddProperty(property.Name, property.PropertyType);
            }
            return classGenerator.GetGeneratedCode();

        }

    }
}
