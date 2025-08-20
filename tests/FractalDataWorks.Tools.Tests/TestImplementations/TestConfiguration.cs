using FluentValidation.Results;

namespace FractalDataWorks.Tools.Tests.TestImplementations;

/// <summary>
/// Test implementation of IFdwConfiguration for testing purposes.
/// </summary>
public sealed class TestConfiguration : IFdwConfiguration
{
    public string SectionName { get; set; } = "TestConfiguration";
    
    public string Name { get; set; } = "TestConfiguration";
    
    public string? ConnectionString { get; set; }
    
    public int Timeout { get; set; } = 30;
    
    public ValidationResult Validate()
    {
        return new ValidationResult();
    }
}