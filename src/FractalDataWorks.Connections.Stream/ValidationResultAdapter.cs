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