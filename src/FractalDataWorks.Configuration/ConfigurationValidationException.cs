using System;
using System.Runtime.Serialization;

namespace FractalDataWorks.Configuration
{
    /// <summary>
    /// Exception thrown when configuration validation fails
    /// </summary>
    [Serializable]
    public class ConfigurationValidationException : Exception
    {
        /// <summary>
        /// Creates a new configuration validation exception
        /// </summary>
        public ConfigurationValidationException()
        {
        }

        /// <summary>
        /// Creates a new configuration validation exception with a message
        /// </summary>
        public ConfigurationValidationException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new configuration validation exception with a message and inner exception
        /// </summary>
        public ConfigurationValidationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new configuration validation exception from serialization
        /// </summary>
        protected ConfigurationValidationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}