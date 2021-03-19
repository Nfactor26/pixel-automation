using System;

namespace Pixel.Automation.Core.Exceptions
{
    [Serializable]
    public class ScriptExecutionException : Exception
    {
        public ScriptExecutionException() : base()
        {

        }
        
        public ScriptExecutionException(string message) : base(message)
        {
        }

        public ScriptExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
