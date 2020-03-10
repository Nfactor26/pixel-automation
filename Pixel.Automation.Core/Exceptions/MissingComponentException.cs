using System;

namespace Pixel.Automation.Core.Exceptions
{
    public class MissingComponentException : Exception
    {
        public MissingComponentException() : base()
        {
        }

        public MissingComponentException(string message) : base(message)
        {

        }

        public MissingComponentException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
