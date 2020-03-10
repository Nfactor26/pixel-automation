using System;
using System.Runtime.Serialization;

//Code borrowed from awesome scriptcs project

namespace Pixel.Automation.Core.Exceptions
{
    [Serializable]
    public class ScriptExecutionException : Exception
    {
        public ScriptExecutionException(string message)
            : base(message)
        {
        }

        public ScriptExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ScriptExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
