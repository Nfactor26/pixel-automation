using System;
using System.Runtime.Serialization;

//Code borrowed from awesome scriptcs project


namespace Pixel.Automation.Core.Exceptions
{
    [Serializable]
    public class ScriptCompilationException : Exception
    {
        public ScriptCompilationException(string message)
            : base(message)
        {
        }

        public ScriptCompilationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ScriptCompilationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
