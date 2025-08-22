using System;

namespace FractalDataWorks.CodeBuilder.Analysis.CSharp.Attributes;

/// <summary>
/// Attribute used to mark classes for code generation in tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateCodeAttribute : Attribute
{
}
