using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.SecretManagement.Abstractions.Tests;

/// <summary>
/// Essential tests for SecretValue class to achieve code coverage.
/// </summary>
public sealed class SimpleSecretValueTests : IDisposable
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
    public void StringConstructorShouldCreateSecretValueCorrectly()
    {
        // Arrange
        const string key = "test-key";
        const string value = "test-secret-value";

        // Act
        var secret = new SecretValue(key, value);
        _secretsToDispose.Add(secret);

        // Assert
        secret.Key.ShouldBe(key);
        secret.IsBinary.ShouldBeFalse();
        secret.Metadata.ShouldNotBeNull();
    }

    [Fact]
    public void BinaryConstructorShouldCreateSecretValueCorrectly()
    {
        // Arrange
        const string key = "binary-key";
        var value = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var secret = new SecretValue(key, value);
        _secretsToDispose.Add(secret);

        // Assert
        secret.Key.ShouldBe(key);
        secret.IsBinary.ShouldBeTrue();
        secret.Metadata.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void StringConstructorShouldThrowWhenKeyIsInvalid(string invalidKey)
    {
        Should.Throw<ArgumentException>(() => new SecretValue(invalidKey, "value"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void BinaryConstructorShouldThrowWhenKeyIsInvalid(string invalidKey)
    {
        Should.Throw<ArgumentException>(() => new SecretValue(invalidKey, new byte[] { 0x01 }));
    }

    [Fact]
    public void GetStringValueShouldReturnOriginalValue()
    {
        // Arrange
        const string originalValue = "original-secret";
        var secret = new SecretValue("key", originalValue);
        _secretsToDispose.Add(secret);

        // Act
        var retrievedValue = secret.GetStringValue();

        // Assert
        retrievedValue.ShouldBe(originalValue);
    }

    [Fact]
    public void GetBinaryValueShouldReturnOriginalValue()
    {
        // Arrange
        var originalValue = new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f };
        var secret = new SecretValue("key", originalValue);
        _secretsToDispose.Add(secret);

        // Act
        var retrievedValue = secret.GetBinaryValue();

        // Assert
        retrievedValue.ShouldBe(originalValue);
    }

    [Fact]
    public void GetStringValueShouldThrowForBinarySecret()
    {
        // Arrange
        var secret = new SecretValue("key", new byte[] { 0x01 });
        _secretsToDispose.Add(secret);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => secret.GetStringValue());
    }

    [Fact]
    public void GetBinaryValueShouldThrowForStringSecret()
    {
        // Arrange
        var secret = new SecretValue("key", "value");
        _secretsToDispose.Add(secret);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => secret.GetBinaryValue());
    }

    [Fact]
    public void AccessStringValueShouldExecuteCallback()
    {
        // Arrange
        var secret = new SecretValue("key", "test-value");
        _secretsToDispose.Add(secret);

        // Act
        var result = secret.AccessStringValue(value => value.Length);

        // Assert
        result.ShouldBe(10);
    }

    [Fact]
    public void AccessBinaryValueShouldExecuteCallback()
    {
        // Arrange
        var secret = new SecretValue("key", new byte[] { 0x01, 0x02, 0x03 });
        _secretsToDispose.Add(secret);

        // Act
        var result = secret.AccessBinaryValue(value => value.Length);

        // Assert
        result.ShouldBe(3);
    }

    [Fact]
    public void AccessStringValueShouldThrowWhenAccessorIsNull()
    {
        // Arrange
        var secret = new SecretValue("key", "value");
        _secretsToDispose.Add(secret);

        // Act & Assert
        Should.Throw<NullReferenceException>(() => secret.AccessStringValue((Func<string, string>)null!));
    }

    [Fact]
    public void IsExpiredShouldReturnTrueWhenExpired()
    {
        // Arrange
        var pastDate = DateTimeOffset.UtcNow.AddDays(-1);
        var secret = new SecretValue("key", "value", expiresAt: pastDate);
        _secretsToDispose.Add(secret);

        // Act & Assert
        secret.IsExpired.ShouldBeTrue();
    }

    [Fact]
    public void IsExpiredShouldReturnFalseWhenNotExpired()
    {
        // Arrange
        var futureDate = DateTimeOffset.UtcNow.AddDays(1);
        var secret = new SecretValue("key", "value", expiresAt: futureDate);
        _secretsToDispose.Add(secret);

        // Act & Assert
        secret.IsExpired.ShouldBeFalse();
    }

    [Fact]
    public void DisposeShouldMakeMethodsThrow()
    {
        // Arrange
        var secret = new SecretValue("key", "value");

        // Act
        secret.Dispose();

        // Assert
        Should.Throw<ObjectDisposedException>(() => secret.GetStringValue());
    }

    [Fact]
    public void DisposeShouldBeIdempotent()
    {
        // Arrange
        var secret = new SecretValue("key", "value");

        // Act & Assert - Should not throw
        secret.Dispose();
        secret.Dispose();
        secret.Dispose();
    }
}