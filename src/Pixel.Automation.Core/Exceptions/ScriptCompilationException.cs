using System;

namespace Pixel.Automation.Core.Exceptions
{
    [Serializable]
    public class ScriptCompilationException : Exception
    {
        public ScriptCompilationException() : base()
        {

        }

        public ScriptCompilationException(string message) : base(message)
        {
        }

        public ScriptCompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
