using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Tests.ExtendedEnums;

/// <summary>
/// Concrete test implementation of TestStatusOptionBase for unit testing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test class - no business logic to test, only validates framework functionality")]
public sealed class ConcreteTestStatusOption : TestStatusOptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcreteTestStatusOption"/> class.
    /// </summary>
    /// <param name="enumValue">The underlying TestStatus enum value.</param>
    public ConcreteTestStatusOption(TestStatus enumValue) : base(enumValue)
    {
    }
}