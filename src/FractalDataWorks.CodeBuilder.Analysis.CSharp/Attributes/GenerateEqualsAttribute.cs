using System;

namespace FractalDataWorks.CodeBuilder.Analysis.CSharp.Attributes;

/// <summary>
/// Attribute used to mark classes for generating Equals and GetHashCode methods in tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateEqualsAttribute : Attribute
{
}
