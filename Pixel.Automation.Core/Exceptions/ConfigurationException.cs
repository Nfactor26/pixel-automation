using System;

namespace Pixel.Automation.Core.Exceptions
{
    /// <summary>
    /// ConfigurationException is thrown when there is an issue with some configuration.
    /// </summary>
    public class ConfigurationException : Exception
    {
        public ConfigurationException()
        {

        }

        public ConfigurationException(string message) : base(message)
        {

        }

        public ConfigurationException(string message, Exception innerException) : base(message,innerException)
        {

        }
    }
}
