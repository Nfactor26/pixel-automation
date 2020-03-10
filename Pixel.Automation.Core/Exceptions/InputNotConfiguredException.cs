using System;

namespace Pixel.Automation.Core.Exceptions
{
    public class InputNotConfiguredException : Exception
    {
        public InputNotConfiguredException()
        {

        }

        public InputNotConfiguredException(string message) : base(message)
        {

        }

        public InputNotConfiguredException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
