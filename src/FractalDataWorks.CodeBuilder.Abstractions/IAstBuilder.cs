using System;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Base interface for all AST builders. Builders create immutable AST products.
/// This implements the true Builder pattern where the builder is separate from the product.
/// </summary>
/// <typeparam name="TNode">The type of AST node this builder creates.</typeparam>
public interface IAstBuilder<out TNode> where TNode : class, IAstNode
{
    /// <summary>
    /// Builds the immutable AST node product.
    /// </summary>
    /// <returns>The immutable AST node.</returns>
    TNode Build();
}

/// <summary>
/// Interface for builders that support cloning their current state.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type.</typeparam>
/// <typeparam name="TNode">The type of AST node this builder creates.</typeparam>
public interface ICloneableBuilder<out TBuilder, out TNode> : IAstBuilder<TNode>
    where TBuilder : ICloneableBuilder<TBuilder, TNode>
    where TNode : class, IAstNode
{
    /// <summary>
    /// Creates a clone of this builder with the same state.
    /// </summary>
    /// <returns>A new builder instance with the same state as this one.</returns>
    TBuilder Clone();
}

/// <summary>
/// Interface for builders that can be validated before building.
/// </summary>
public interface IValidatableBuilder
{
    /// <summary>
    /// Validates the current builder state.
    /// </summary>
    /// <returns>True if the builder state is valid; otherwise, false.</returns>
    bool IsValid { get; }

    /// <summary>
    /// Gets validation errors for the current builder state.
    /// </summary>
    /// <returns>An array of validation error messages.</returns>
    string[] GetValidationErrors();
}

/// <summary>
/// Interface for builders that support conditional building.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type.</typeparam>
public interface IConditionalBuilder<out TBuilder>
{
    /// <summary>
    /// Conditionally applies a configuration to the builder.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="configure">The configuration to apply if the condition is true.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder When(bool condition, Func<TBuilder, TBuilder> configure);

    /// <summary>
    /// Conditionally applies a configuration to the builder based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to evaluate.</param>
    /// <param name="configure">The configuration to apply if the predicate returns true.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder When(Func<bool> predicate, Func<TBuilder, TBuilder> configure);
}