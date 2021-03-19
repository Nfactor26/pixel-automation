using System;

namespace Pixel.Automation.Core.Exceptions
{
    public class ModelDeserializationException : Exception
    {
        public ModelDeserializationException()
        {
        }

        public ModelDeserializationException(string message)
            : base(message)
        {
        }

        public ModelDeserializationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
