using Dawn;
using Pixel.Automation.Core.Interfaces.Scripting;
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
       
    }
}
