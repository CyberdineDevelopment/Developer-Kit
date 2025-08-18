using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentValidation;
using FractalDataWorks.Configuration;
using FractalDataWorks.Data;
using FractalDataWorks.Services.DataProvider.Abstractions;

namespace FractalDataWorks.Services.DataProvider.MsSql.Configuration;

/// <summary>
/// Configuration for Microsoft SQL Server data provider.
/// </summary>
public sealed class MsSqlConfiguration : ConfigurationBase<MsSqlConfiguration>, IDataConfiguration, IDataProvidersConfiguration
{
    /// <summary>
    /// Gets or sets the connection string for the SQL Server database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public string ProviderType { get; set; } = "SqlServer";

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets a value indicating whether to enable connection pooling.
    /// </summary>
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum pool size.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether to enable automatic retry.
    /// </summary>
    public bool EnableAutoRetry { get; set; } = true;

    /// <summary>
    /// Gets or sets the retry policy configuration.
    /// </summary>
    public IDataRetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the schema mapping dictionary.
    /// Maps logical schema names to physical schema names.
    /// </summary>
    public IDictionary<string, string> SchemaMapping { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the default schema name.
    /// </summary>
    public string DefaultSchema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server name or address.
    /// </summary>
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the port number for the SQL Server instance.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Gets or sets the instance name for named instances.
    /// </summary>
    public string? InstanceName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use Windows Authentication.
    /// When false, SQL Server Authentication is used.
    /// </summary>
    public bool UseWindowsAuthentication { get; set; } = true;

    /// <summary>
    /// Gets or sets the username for SQL Server Authentication.
    /// Only used when UseWindowsAuthentication is false.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for SQL Server Authentication.
    /// Only used when UseWindowsAuthentication is false.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to encrypt the connection.
    /// </summary>
    public bool EncryptConnection { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to trust the server certificate.
    /// </summary>
    public bool TrustServerCertificate { get; set; }

    /// <summary>
    /// Gets or sets the application name to use in the connection.
    /// </summary>
    public string ApplicationName { get; set; } = "FractalDataWorks";

    /// <summary>
    /// Gets or sets a value indicating whether to enable Multiple Active Result Sets (MARS).
    /// </summary>
    public bool EnableMars { get; set; }

    /// <summary>
    /// Gets or sets the workstation ID to use in the connection.
    /// </summary>
    public string? WorkstationId { get; set; }

    /// <summary>
    /// Gets or sets the packet size for network communications.
    /// </summary>
    public int? PacketSize { get; set; }

    /// <summary>
    /// Gets or sets additional connection string parameters.
    /// </summary>
    public IDictionary<string, string> AdditionalParameters { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <inheritdoc/>
    public override string SectionName => "MsSqlDataProvider";

    /// <inheritdoc/>
    protected override IValidator<MsSqlConfiguration>? GetValidator()
    {
        return new MsSqlConfigurationValidator();
    }

    /// <summary>
    /// Gets the full server address including instance and port.
    /// </summary>
    /// <returns>The formatted server address.</returns>
    public string GetServerAddress()
    {
        var serverAddress = ServerName;
        
        if (!string.IsNullOrEmpty(InstanceName))
        {
            serverAddress += $"\\{InstanceName}";
        }
        else if (Port.HasValue)
        {
            serverAddress += $",{Port.Value}";
        }
        
        return serverAddress;
    }

    /// <summary>
    /// Builds the connection string from the configuration properties.
    /// </summary>
    /// <returns>The built connection string.</returns>
    public string BuildConnectionString()
    {
        if (!string.IsNullOrEmpty(ConnectionString))
        {
            return ConnectionString;
        }

        var builder = new Dictionary<string, string>(StringComparer.Ordinal);
        
        AddServerAndDatabaseSettings(builder);
        AddAuthenticationSettings(builder);
        AddConnectionSettings(builder);
        AddSecuritySettings(builder);
        AddOptionalSettings(builder);
        AddAdditionalParameters(builder);

        return string.Join(";", builder.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }

    /// <summary>
    /// Adds server and database settings to the connection string builder.
    /// </summary>
    /// <param name="builder">The connection string builder dictionary.</param>
    private void AddServerAndDatabaseSettings(IDictionary<string, string> builder)
    {
        if (!string.IsNullOrEmpty(ServerName))
        {
            builder["Server"] = GetServerAddress();
        }
        
        if (!string.IsNullOrEmpty(DatabaseName))
        {
            builder["Database"] = DatabaseName;
        }
    }

    /// <summary>
    /// Adds authentication settings to the connection string builder.
    /// </summary>
    /// <param name="builder">The connection string builder dictionary.</param>
    private void AddAuthenticationSettings(IDictionary<string, string> builder)
    {
        if (UseWindowsAuthentication)
        {
            builder["Integrated Security"] = "true";
        }
        else
        {
            if (!string.IsNullOrEmpty(Username))
            {
                builder["User ID"] = Username;
            }
            if (!string.IsNullOrEmpty(Password))
            {
                builder["Password"] = Password;
            }
        }
    }

    /// <summary>
    /// Adds connection settings to the connection string builder.
    /// </summary>
    /// <param name="builder">The connection string builder dictionary.</param>
    private void AddConnectionSettings(IDictionary<string, string> builder)
    {
        builder["Connect Timeout"] = ConnectionTimeoutSeconds.ToString(CultureInfo.InvariantCulture);
        builder["Command Timeout"] = CommandTimeoutSeconds.ToString(CultureInfo.InvariantCulture);
        builder["Pooling"] = EnableConnectionPooling.ToString(CultureInfo.InvariantCulture);
        
        if (EnableConnectionPooling)
        {
            builder["Max Pool Size"] = MaxPoolSize.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Adds security settings to the connection string builder.
    /// </summary>
    /// <param name="builder">The connection string builder dictionary.</param>
    private void AddSecuritySettings(IDictionary<string, string> builder)
    {
        builder["Encrypt"] = EncryptConnection.ToString(CultureInfo.InvariantCulture);
        builder["TrustServerCertificate"] = TrustServerCertificate.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Adds optional settings to the connection string builder.
    /// </summary>
    /// <param name="builder">The connection string builder dictionary.</param>
    private void AddOptionalSettings(IDictionary<string, string> builder)
    {
        if (!string.IsNullOrEmpty(ApplicationName))
        {
            builder["Application Name"] = ApplicationName;
        }

        if (EnableMars)
        {
            builder["MultipleActiveResultSets"] = "true";
        }

        if (!string.IsNullOrEmpty(WorkstationId))
        {
            builder["Workstation ID"] = WorkstationId;
        }

        if (PacketSize.HasValue)
        {
            builder["Packet Size"] = PacketSize.Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Adds additional parameters to the connection string builder.
    /// </summary>
    /// <param name="builder">The connection string builder dictionary.</param>
    private void AddAdditionalParameters(IDictionary<string, string> builder)
    {
        foreach (var kvp in AdditionalParameters)
        {
            builder[kvp.Key] = kvp.Value;
        }
    }

    /// <inheritdoc/>
    protected override void CopyTo(MsSqlConfiguration target)
    {
        base.CopyTo(target);
        
        target.ConnectionString = ConnectionString;
        target.ProviderType = ProviderType;
        target.ConnectionTimeoutSeconds = ConnectionTimeoutSeconds;
        target.CommandTimeoutSeconds = CommandTimeoutSeconds;
        target.EnableConnectionPooling = EnableConnectionPooling;
        target.MaxPoolSize = MaxPoolSize;
        target.EnableAutoRetry = EnableAutoRetry;
        target.RetryPolicy = RetryPolicy;
        target.SchemaMapping = new Dictionary<string, string>(SchemaMapping, StringComparer.Ordinal);
        target.DefaultSchema = DefaultSchema;
        target.DatabaseName = DatabaseName;
        target.ServerName = ServerName;
        target.Port = Port;
        target.InstanceName = InstanceName;
        target.UseWindowsAuthentication = UseWindowsAuthentication;
        target.Username = Username;
        target.Password = Password;
        target.EncryptConnection = EncryptConnection;
        target.TrustServerCertificate = TrustServerCertificate;
        target.ApplicationName = ApplicationName;
        target.EnableMars = EnableMars;
        target.WorkstationId = WorkstationId;
        target.PacketSize = PacketSize;
        target.AdditionalParameters = new Dictionary<string, string>(AdditionalParameters, StringComparer.Ordinal);
    }
}