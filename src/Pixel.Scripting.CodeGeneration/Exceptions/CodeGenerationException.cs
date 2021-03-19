using System;

namespace Pixel.Scripting.CodeGeneration.Exceptions
{
    public class CodeGenerationException : Exception
    {
        public CodeGenerationException() : base()
        {

        }

        public CodeGenerationException(string message) : base(message)
        {

        }

        public CodeGenerationException(string message, Exception innerExcpetion) : base(message, innerExcpetion)
        {

        }
    }
}
