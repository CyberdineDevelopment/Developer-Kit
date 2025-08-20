using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Tests.ExtendedEnums;

/// <summary>
/// Test enum used for validating ExtendedEnums functionality.
/// </summary>
public enum TestStatus
{
    /// <summary>Pending status with value 1.</summary>
    Pending = 1,
    
    /// <summary>Processing status with value 2.</summary>
    Processing = 2,
    
    /// <summary>Completed status with value 3.</summary>
    Completed = 3,
    
    /// <summary>Failed status with value 4.</summary>
    Failed = 4
}