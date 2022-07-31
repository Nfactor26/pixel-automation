using System;

namespace Pixel.Automation.Core.Exceptions
{
    public class ArgumentNotConfiguredException : Exception
    {
        public ArgumentNotConfiguredException()
        {

        }

        public ArgumentNotConfiguredException(string message) : base(message)
        {

        }

        public ArgumentNotConfiguredException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
