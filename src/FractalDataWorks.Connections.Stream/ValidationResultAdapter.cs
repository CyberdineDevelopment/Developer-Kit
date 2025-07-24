using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using FractalDataWorks.Validation;

namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Adapter to convert FluentValidation results to IValidationResult.
/// </summary>
internal class ValidationResultAdapter : IValidationResult
{
    private readonly ValidationResult _validationResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResultAdapter"/> class.
    /// </summary>
    /// <param name="validationResult">The FluentValidation result.</param>
    public ValidationResultAdapter(ValidationResult validationResult)
    {
        _validationResult = validationResult;
    }

    /// <inheritdoc/>
    public bool IsValid => _validationResult.IsValid;

    /// <inheritdoc/>
    public IReadOnlyList<IValidationError> Errors => 
        _validationResult.Errors.Select(e => new ValidationErrorAdapter(e)).ToList();
}

/// <summary>
/// Adapter to convert FluentValidation errors to IValidationError.
/// </summary>
internal class ValidationErrorAdapter : IValidationError
{
    private readonly ValidationFailure _failure;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationErrorAdapter"/> class.
    /// </summary>
    /// <param name="failure">The validation failure.</param>
    public ValidationErrorAdapter(ValidationFailure failure)
    {
        _failure = failure;
    }

    /// <inheritdoc/>
    public string PropertyName => _failure.PropertyName;

    /// <inheritdoc/>
    public string ErrorMessage => _failure.ErrorMessage;

    /// <inheritdoc/>
    public object? AttemptedValue => _failure.AttemptedValue;

    /// <inheritdoc/>
    public string ErrorCode => _failure.ErrorCode;

    /// <inheritdoc/>
    public object? CustomState => _failure.CustomState;

    /// <inheritdoc/>
    public ValidationSeverity Severity => _failure.Severity switch
    {
        FluentValidation.Severity.Error => ValidationSeverity.Error,
        FluentValidation.Severity.Warning => ValidationSeverity.Warning,
        FluentValidation.Severity.Info => ValidationSeverity.Info,
        _ => ValidationSeverity.Error
    };
}