using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.MsSql.Configuration;
using FractalDataWorks.Services.DataProvider.MsSql.Services;
using FractalDataWorks.Services.EnhancedEnums;

namespace FractalDataWorks.Services.DataProvider.MsSql.EnhancedEnums;

/// <summary>
/// Enhanced enum service type for Microsoft SQL Server data provider configurations.
/// </summary>
public sealed class MsSqlDataProviderServiceType 
    : ServiceTypeOptionBase<
        MsSqlDataProviderServiceType,
        MsSqlDataProvider,
        MsSqlConfiguration,
        MsSqlDataProviderFactory>
{
    /// <summary>
    /// Default SQL Server configuration for general-purpose use.
    /// </summary>
    public static readonly MsSqlDataProviderServiceType Default = new(1, "Default");

    /// <summary>
    /// Read-only SQL Server configuration optimized for query operations.
    /// </summary>
    public static readonly MsSqlDataProviderServiceType ReadOnly = new(2, "ReadOnly");

    /// <summary>
    /// High-performance SQL Server configuration with optimized connection settings.
    /// </summary>
    public static readonly MsSqlDataProviderServiceType HighPerformance = new(3, "HighPerformance");

    /// <summary>
    /// Reporting configuration optimized for long-running queries and large result sets.
    /// </summary>
    public static readonly MsSqlDataProviderServiceType Reporting = new(4, "Reporting");

    /// <summary>
    /// Transaction configuration optimized for transactional operations with proper isolation.
    /// </summary>
    public static readonly MsSqlDataProviderServiceType Transactional = new(5, "Transactional");

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlDataProviderServiceType"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type option.</param>
    /// <param name="name">The name of this service type option.</param>
    private MsSqlDataProviderServiceType(int id, string name) : base(id, name)
    {
    }

    /// <summary>
    /// Gets the configuration section name for MsSql data provider services.
    /// </summary>
    protected override string ConfigurationSection => "DataProviders:MsSql:Configurations";
}