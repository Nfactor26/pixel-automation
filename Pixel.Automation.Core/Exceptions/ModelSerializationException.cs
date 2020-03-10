using System;

namespace Pixel.Automation.Core.Exceptions
{
    public class ModelSerializationException : Exception
    {
        public ModelSerializationException()
        {
        }

        public ModelSerializationException(string message)
            : base(message)
        {
        }

        public ModelSerializationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
