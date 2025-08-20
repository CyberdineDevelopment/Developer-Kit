using System.Diagnostics.CodeAnalysis;
using FractalDataWorks.EnhancedEnums.ExtendedEnums;

namespace FractalDataWorks.EnhancedEnums.Tests.ExtendedEnums;

/// <summary>
/// Test implementation of ExtendedEnumOptionBase for testing purposes.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test class - no business logic to test, only validates framework functionality")]
public abstract class TestStatusOptionBase : ExtendedEnumOptionBase<TestStatusOptionBase, TestStatus>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestStatusOptionBase"/> class.
    /// </summary>
    /// <param name="enumValue">The underlying TestStatus enum value.</param>
    protected TestStatusOptionBase(TestStatus enumValue) : base(enumValue)
    {
    }
}