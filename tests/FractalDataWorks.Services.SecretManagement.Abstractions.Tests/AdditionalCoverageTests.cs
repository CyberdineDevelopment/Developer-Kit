using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.SecretManagement.Abstractions.Tests;

/// <summary>
/// Additional tests to improve code coverage for edge cases and missing paths.
/// </summary>
public sealed class AdditionalCoverageTests : IDisposable
{
    private readonly List<SecretValue> _secretsToDispose = new();

    public void Dispose()
    {
        foreach (var secret in _secretsToDispose)
        {
            secret.Dispose();
        }
        _secretsToDispose.Clear();
    }

    [Fact]
    public void AccessBinaryValueShouldThrowWithNullAccessor()
    {
        // Arrange
        var secret = new SecretValue("key", new byte[] { 0x01 });
        _secretsToDispose.Add(secret);

        // Act & Assert
        Should.Throw<NullReferenceException>(() => secret.AccessBinaryValue((Func<byte[], int>)null!));
    }

    [Fact]
    public void SecretValueWithAllParametersShouldInitializeCorrectly()
    {
        // Arrange
        const string key = "comprehensive-key";
        const string value = "comprehensive-value";
        const string version = "v1.2.3";
        var createdAt = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var modifiedAt = new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var expiresAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { "environment", "test" },
            { "source", "unit-test" }
        };

        // Act
        var secret = new SecretValue(key, value, version, createdAt, modifiedAt, expiresAt, metadata);
        _secretsToDispose.Add(secret);

        // Assert
        secret.Key.ShouldBe(key);
        secret.Version.ShouldBe(version);
        secret.CreatedAt.ShouldBe(createdAt);
        secret.ModifiedAt.ShouldBe(modifiedAt);
        secret.ExpiresAt.ShouldBe(expiresAt);
        secret.Metadata.ShouldBe(metadata);
        secret.IsBinary.ShouldBeFalse();
        secret.IsExpired.ShouldBeTrue(); // Should be expired since expiresAt is in the past
    }

    [Fact]
    public void BinarySecretValueWithAllParametersShouldInitializeCorrectly()
    {
        // Arrange
        const string key = "comprehensive-binary-key";
        var value = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        const string version = "v2.0.0";
        var createdAt = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var modifiedAt = new DateTimeOffset(2023, 6, 2, 0, 0, 0, TimeSpan.Zero);
        var expiresAt = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { "type", "certificate" },
            { "format", "pkcs12" }
        };

        // Act
        var secret = new SecretValue(key, value, version, createdAt, modifiedAt, expiresAt, metadata);
        _secretsToDispose.Add(secret);

        // Assert
        secret.Key.ShouldBe(key);
        secret.Version.ShouldBe(version);
        secret.CreatedAt.ShouldBe(createdAt);
        secret.ModifiedAt.ShouldBe(modifiedAt);
        secret.ExpiresAt.ShouldBe(expiresAt);
        secret.Metadata.ShouldBe(metadata);
        secret.IsBinary.ShouldBeTrue();
        secret.IsExpired.ShouldBeFalse();
    }

    [Fact]
    public void SecretValueShouldHandleNullMetadata()
    {
        // Arrange
        const string key = "null-metadata-key";
        const string value = "test-value";

        // Act
        var secret = new SecretValue(key, value, metadata: null);
        _secretsToDispose.Add(secret);

        // Assert
        secret.Metadata.ShouldNotBeNull();
        secret.Metadata.Count.ShouldBe(0);
    }

    [Fact]
    public void BinarySecretValueShouldHandleNullMetadata()
    {
        // Arrange
        const string key = "null-metadata-binary-key";
        var value = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var secret = new SecretValue(key, value, metadata: null);
        _secretsToDispose.Add(secret);

        // Assert
        secret.Metadata.ShouldNotBeNull();
        secret.Metadata.Count.ShouldBe(0);
    }

    [Fact]
    public void IsExpiredShouldReturnFalseForNullExpiration()
    {
        // Arrange
        const string key = "no-expiry-key";
        const string value = "no-expiry-value";
        var secret = new SecretValue(key, value, expiresAt: null);
        _secretsToDispose.Add(secret);

        // Act & Assert
        secret.IsExpired.ShouldBeFalse();
        secret.ExpiresAt.ShouldBeNull();
    }

    [Fact]
    public void GetStringValueAfterDisposeShouldThrow()
    {
        // Arrange
        var secret = new SecretValue("key", "value");
        secret.Dispose();

        // Act & Assert
        Should.Throw<ObjectDisposedException>(() => secret.GetStringValue());
    }

    [Fact]
    public void GetBinaryValueAfterDisposeShouldThrow()
    {
        // Arrange
        var secret = new SecretValue("key", new byte[] { 0x01 });
        secret.Dispose();

        // Act & Assert
        Should.Throw<ObjectDisposedException>(() => secret.GetBinaryValue());
    }

    [Fact]
    public void AccessStringValueAfterDisposeShouldThrow()
    {
        // Arrange
        var secret = new SecretValue("key", "value");
        secret.Dispose();

        // Act & Assert
        Should.Throw<ObjectDisposedException>(() => secret.AccessStringValue(v => v.Length));
    }

    [Fact]
    public void AccessBinaryValueAfterDisposeShouldThrow()
    {
        // Arrange
        var secret = new SecretValue("key", new byte[] { 0x01 });
        secret.Dispose();

        // Act & Assert
        Should.Throw<ObjectDisposedException>(() => secret.AccessBinaryValue(v => v.Length));
    }

    [Fact]
    public void PropertiesShouldRemainAccessibleAfterDispose()
    {
        // Arrange
        const string key = "properties-test";
        const string version = "v1.0";
        var secret = new SecretValue(key, "value", version);
        
        // Act
        secret.Dispose();

        // Assert - Properties should still be accessible
        secret.Key.ShouldBe(key);
        secret.Version.ShouldBe(version);
        secret.IsBinary.ShouldBeFalse();
        secret.Metadata.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("simple-secret")]
    [InlineData("Complex-P@ssw0rd!")]
    [InlineData("Multi\nLine\nSecret")]
    [InlineData("Unicode: 你好世界 🔐")]
    public void StringSecretShouldHandleVariousValues(string testValue)
    {
        // Arrange
        var secret = new SecretValue("variable-test", testValue);
        _secretsToDispose.Add(secret);

        // Act
        var retrievedValue = secret.GetStringValue();

        // Assert
        retrievedValue.ShouldBe(testValue);
    }

    [Fact]
    public void SecretValueWithComplexMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { "string", "value" },
            { "number", 42 },
            { "boolean", true },
            { "date", DateTimeOffset.Now },
            { "array", new[] { "a", "b", "c" } },
            { "nested", new Dictionary<string, object>(StringComparer.Ordinal) { { "inner", "value" } } }
        };

        // Act
        var secret = new SecretValue("complex-metadata", "value", metadata: metadata);
        _secretsToDispose.Add(secret);

        // Assert
        secret.Metadata.Count.ShouldBe(6);
        secret.Metadata["string"].ShouldBe("value");
        secret.Metadata["number"].ShouldBe(42);
        secret.Metadata["boolean"].ShouldBe(true);
    }
}