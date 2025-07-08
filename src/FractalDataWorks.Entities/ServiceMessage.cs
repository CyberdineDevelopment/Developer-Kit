using System;
using System.Collections.Generic;
using FractalDataWorks;

namespace FractalDataWorks.Entities
{
    /// <summary>
    /// Represents a message from a service operation
    /// </summary>
    public record ServiceMessage : IServiceMessage
    {
        /// <summary>
        /// The message text
        /// </summary>
        public string Message { get; init; } = string.Empty;
        
        /// <summary>
        /// The severity level of the message
        /// </summary>
        public MessageSeverity Severity { get; init; }
        
        /// <summary>
        /// Optional error code
        /// </summary>
        public string Code { get; init; } = string.Empty;
        
        /// <summary>
        /// When the message was created
        /// </summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        
        /// <summary>
        /// Optional additional context data
        /// </summary>
        public Dictionary<string, object>? Context { get; init; }
        
        /// <summary>
        /// Creates an error message
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="code">Optional error code</param>
        /// <param name="context">Optional context data</param>
        /// <returns>A service message with Error severity</returns>
        public static ServiceMessage Error(string message, string code = "", Dictionary<string, object>? context = null) 
            => new() { Message = message, Severity = MessageSeverity.Error, Code = code, Context = context };
        
        /// <summary>
        /// Creates a warning message
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="code">Optional code</param>
        /// <param name="context">Optional context data</param>
        /// <returns>A service message with Warning severity</returns>
        public static ServiceMessage Warning(string message, string code = "", Dictionary<string, object>? context = null) 
            => new() { Message = message, Severity = MessageSeverity.Warning, Code = code, Context = context };
        
        /// <summary>
        /// Creates an info message
        /// </summary>
        /// <param name="message">The info message</param>
        /// <param name="code">Optional code</param>
        /// <param name="context">Optional context data</param>
        /// <returns>A service message with Info severity</returns>
        public static ServiceMessage Info(string message, string code = "", Dictionary<string, object>? context = null) 
            => new() { Message = message, Severity = MessageSeverity.Info, Code = code, Context = context };
        
        /// <summary>
        /// Creates a fatal message
        /// </summary>
        /// <param name="message">The fatal message</param>
        /// <param name="code">Optional code</param>
        /// <param name="context">Optional context data</param>
        /// <returns>A service message with Fatal severity</returns>
        public static ServiceMessage Fatal(string message, string code = "", Dictionary<string, object>? context = null) 
            => new() { Message = message, Severity = MessageSeverity.Fatal, Code = code, Context = context };
        
        /// <summary>
        /// Implicit conversion from string to ServiceMessage (creates an Info message)
        /// </summary>
        /// <param name="message">The message text</param>
        /// <returns>A service message with Info severity</returns>
        public static implicit operator ServiceMessage(string message) 
            => Info(message);
        
        /// <summary>
        /// String representation
        /// </summary>
        /// <returns>The message text</returns>
        public override string ToString() => Message;
    }
}