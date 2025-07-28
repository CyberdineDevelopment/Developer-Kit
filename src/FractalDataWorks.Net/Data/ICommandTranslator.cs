namespace FractalDataWorks.Data;

/// <summary>
/// Interface for bidirectional translation between commands and native formats
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TNative">The native format type (e.g., SqlCommand, HttpRequest)</typeparam>
public interface ICommandTranslator<TCommand, TNative>
    where TCommand : IDataCommand
{
    /// <summary>
    /// Translates a command to the native format
    /// </summary>
    /// <param name="command">The command to translate</param>
    /// <returns>The native representation</returns>
    TNative Translate(TCommand command);
    
    /// <summary>
    /// Parses a native format back to a command (reverse engineering)
    /// </summary>
    /// <param name="native">The native representation</param>
    /// <returns>The reconstructed command</returns>
    TCommand Parse(TNative native);
    
    /// <summary>
    /// Validates if a command can be translated
    /// </summary>
    /// <param name="command">The command to validate</param>
    /// <returns>True if the command is valid for translation</returns>
    bool CanTranslate(TCommand command);
}