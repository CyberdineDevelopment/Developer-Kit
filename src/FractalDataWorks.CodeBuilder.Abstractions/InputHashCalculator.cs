using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Provides hash calculation functionality for IInputInfo implementations.
/// </summary>
public static class InputHashCalculator
{
    /// <summary>
    /// Calculates a hash for the given IInputInfo instance.
    /// </summary>
    /// <param name="inputInfo">The input info to calculate hash for.</param>
    /// <returns>A hash string representing the input info.</returns>
    /// <exception cref="ArgumentNullException">Thrown when inputInfo is null.</exception>
    public static string CalculateHash(IInputInfo inputInfo)
    {
        if (inputInfo == null)
        {
            throw new ArgumentNullException(nameof(inputInfo), "Input info cannot be null.");
        }

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        
        inputInfo.WriteToHash(writer);
        writer.Flush();
        
        stream.Position = 0;
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(stream);
        
        return Convert.ToBase64String(hashBytes);
    }
}