using System;
using FluentValidation;

namespace FractalDataWorks.Services.Authentication.AzureEntra.Configuration;

/// <summary>
/// Validator for Azure Entra configuration.
/// </summary>
public sealed class AzureEntraConfigurationValidator : AbstractValidator<AzureEntraConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureEntraConfigurationValidator"/> class.
    /// </summary>
    public AzureEntraConfigurationValidator()
    {
        ConfigureRequiredFieldValidation();
        ConfigureNumericRangeValidation();
        ConfigureOptionalFieldValidation();
    }

    private void ConfigureRequiredFieldValidation()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("ClientId is required")
            .Must(BeValidGuid)
            .WithMessage("ClientId must be a valid GUID");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required")
            .Must(BeValidTenantId)
            .WithMessage("TenantId must be a valid GUID or common tenant identifier");

        RuleFor(x => x.Instance)
            .NotEmpty()
            .WithMessage("Instance is required")
            .Must(BeValidUri)
            .WithMessage("Instance must be a valid HTTPS URL");

        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .WithMessage("RedirectUri is required")
            .Must(BeValidUri)
            .WithMessage("RedirectUri must be a valid URL");

        RuleFor(x => x.Authority)
            .NotEmpty()
            .WithMessage("Authority is required")
            .Must(BeValidUri)
            .WithMessage("Authority must be a valid HTTPS URL");

        RuleFor(x => x.Scopes)
            .NotNull()
            .WithMessage("Scopes cannot be null")
            .Must(HaveValidScopes)
            .WithMessage("Scopes must contain at least one valid scope");

        RuleFor(x => x.ClientType)
            .NotEmpty()
            .WithMessage("ClientType is required")
            .Must(BeValidClientType)
            .WithMessage("ClientType must be either 'Public' or 'Confidential'");

        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .When(x => string.Equals(x.ClientType, "Confidential", StringComparison.OrdinalIgnoreCase))
            .WithMessage("ClientSecret is required for confidential client applications");
    }

    private void ConfigureNumericRangeValidation()
    {
        RuleFor(x => x.TokenCacheLifetimeMinutes)
            .GreaterThan(0)
            .WithMessage("TokenCacheLifetimeMinutes must be greater than 0")
            .LessThanOrEqualTo(1440)
            .WithMessage("TokenCacheLifetimeMinutes cannot exceed 24 hours (1440 minutes)");

        RuleFor(x => x.ClockSkewToleranceMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ClockSkewToleranceMinutes must be 0 or greater")
            .LessThanOrEqualTo(30)
            .WithMessage("ClockSkewToleranceMinutes should not exceed 30 minutes");

        RuleFor(x => x.HttpTimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("HttpTimeoutSeconds must be greater than 0")
            .LessThanOrEqualTo(300)
            .WithMessage("HttpTimeoutSeconds should not exceed 5 minutes (300 seconds)");

        RuleFor(x => x.MaxRetryAttempts)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxRetryAttempts must be 0 or greater")
            .LessThanOrEqualTo(10)
            .WithMessage("MaxRetryAttempts should not exceed 10");
    }

    private void ConfigureOptionalFieldValidation()
    {
        RuleFor(x => x.CacheFilePath)
            .Must(BeValidFilePath)
            .When(x => !string.IsNullOrEmpty(x.CacheFilePath))
            .WithMessage("CacheFilePath must be a valid file path");

        RuleForEach(x => x.AdditionalValidAudiences)
            .Must(BeValidUri)
            .WithMessage("Each additional valid audience must be a valid URI");

        RuleForEach(x => x.AdditionalValidIssuers)
            .Must(BeValidUri)
            .WithMessage("Each additional valid issuer must be a valid URI");
    }

    private static bool BeValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }

    private static bool BeValidTenantId(string value)
    {
        // Common tenant identifiers
        if (string.Equals(value, "common", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "organizations", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "consumers", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // GUID format
        return Guid.TryParse(value, out _);
    }

    private static bool BeValidUri(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && 
               (string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) || 
                string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase));
    }

    private static bool BeValidClientType(string value)
    {
        return string.Equals(value, "Public", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "Confidential", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HaveValidScopes(string[] scopes)
    {
        if (scopes.Length == 0)
        {
            return false;
        }

        // Check that all scopes are non-empty
        foreach (var scope in scopes)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                return false;
            }
        }

        return true;
    }

    private static bool BeValidFilePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return true; // Nullable, so null/empty is valid
        }

        try
        {
            var directory = System.IO.Path.GetDirectoryName(path);
            return directory != null && System.IO.Path.IsPathRooted(path);
        }
        catch
        {
            return false;
        }
    }
}