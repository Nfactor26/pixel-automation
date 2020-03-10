using System;

namespace Pixel.Automation.Core.Exceptions
{
    public class InvalidStateException : Exception
    {
        public InvalidStateException():base()
        {

        }
        public InvalidStateException(string message) : base(message)
        {

        }
        public InvalidStateException(string message,Exception innerException) : base(message,innerException)
        {

        }
    }
}
