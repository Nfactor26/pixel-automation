using System;

namespace Pixel.Automation.Core.Exceptions
{
    public class OutputNotConfiguredException : Exception
    {
        public OutputNotConfiguredException()
        {

        }

        public OutputNotConfiguredException(string message) : base(message)
        {

        }

        public OutputNotConfiguredException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
