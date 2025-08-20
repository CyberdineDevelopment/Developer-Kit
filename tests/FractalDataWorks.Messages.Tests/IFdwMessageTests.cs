using System;
using Xunit;

namespace FractalDataWorks.Messages.Tests;

public class IFdwMessageTests
{

    [Fact]
    public void IFdwMessageShouldHaveExpectedProperties()
    {
        // Arrange
        var messageType = typeof(IFdwMessage);
        // Output($"Testing interface: {messageType.Name}");

        // Act
        var properties = messageType.GetProperties();

        // Assert
        properties.Length.ShouldBe(4);
        
        var severityProperty = messageType.GetProperty("Severity");
        severityProperty.ShouldNotBeNull();
        severityProperty!.PropertyType.ShouldBe(typeof(MessageSeverity));
        severityProperty.CanRead.ShouldBeTrue();
        severityProperty.CanWrite.ShouldBeFalse();

        var messageProperty = messageType.GetProperty("Message");
        messageProperty.ShouldNotBeNull();
        messageProperty!.PropertyType.ShouldBe(typeof(string));
        messageProperty.CanRead.ShouldBeTrue();
        messageProperty.CanWrite.ShouldBeFalse();

        var codeProperty = messageType.GetProperty("Code");
        codeProperty.ShouldNotBeNull();
        codeProperty!.PropertyType.ShouldBe(typeof(string));
        codeProperty.CanRead.ShouldBeTrue();
        codeProperty.CanWrite.ShouldBeFalse();

        var sourceProperty = messageType.GetProperty("Source");
        sourceProperty.ShouldNotBeNull();
        sourceProperty!.PropertyType.ShouldBe(typeof(string));
        sourceProperty.CanRead.ShouldBeTrue();
        sourceProperty.CanWrite.ShouldBeFalse();

        // Output("All properties have expected types and are read-only");
    }

    [Fact]
    public void IFdwMessageShouldBePublicInterface()
    {
        // Arrange & Act
        var messageType = typeof(IFdwMessage);

        // Assert
        messageType.IsInterface.ShouldBeTrue();
        messageType.IsPublic.ShouldBeTrue();
        messageType.IsAbstract.ShouldBeTrue();
        // Output($"{messageType.Name} is a public interface as expected");
    }

    [Theory]
    [InlineData(MessageSeverity.Information, "Test message", "TEST001", "TestSource")]
    [InlineData(MessageSeverity.Warning, "Warning message", null, null)]
    [InlineData(MessageSeverity.Error, "Error message", "ERR002", "ErrorSource")]
    [InlineData(MessageSeverity.Critical, "Critical message", "CRIT003", null)]
    public void TestFdwMessageImplementationShouldWorkCorrectly(MessageSeverity severity, string message, string? code, string? source)
    {
        // Arrange & Act
        var testMessage = new TestFdwMessage(severity, message, code, source);
        // Output($"Created TestFdwMessage: Severity={testMessage.Severity}, Message='{testMessage.Message}', Code='{testMessage.Code}', Source='{testMessage.Source}'");

        // Assert
        testMessage.Severity.ShouldBe(severity);
        testMessage.Message.ShouldBe(message);
        testMessage.Code.ShouldBe(code);
        testMessage.Source.ShouldBe(source);
    }

    [Fact]
    public void TestFdwMessageImplementationShouldHandleNullValues()
    {
        // Arrange & Act
        var testMessage = new TestFdwMessage(MessageSeverity.Error, "Test", null, null);
        // Output($"Created TestFdwMessage with null values: Code='{testMessage.Code}', Source='{testMessage.Source}'");

        // Assert
        testMessage.Code.ShouldBeNull();
        testMessage.Source.ShouldBeNull();
        testMessage.Message.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Valid message")]
    public void TestFdwMessageImplementationShouldAcceptAnyMessageString(string message)
    {
        // Arrange & Act
        var testMessage = new TestFdwMessage(MessageSeverity.Information, message, null, null);
        // Output($"Created TestFdwMessage with message: '{testMessage.Message}'");

        // Assert
        testMessage.Message.ShouldBe(message);
    }

    [Fact]
    public void TestFdwMessageImplementationShouldNotAcceptNullMessage()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new TestFdwMessage(MessageSeverity.Information, null!, null, null))
            .ParamName.ShouldBe("message");
        
        // Output("TestFdwMessage correctly throws ArgumentNullException for null message");
    }

    [Fact]
    public void IFdwMessageShouldSupportPolymorphicUsage()
    {
        // Arrange
        IFdwMessage[] messages = [
            new TestFdwMessage(MessageSeverity.Information, "Info message", "INFO001", "Source1"),
            new TestFdwMessage(MessageSeverity.Warning, "Warning message", "WARN002", "Source2"),
            new TestFdwMessage(MessageSeverity.Error, "Error message", "ERR003", "Source3")
        ];

        // Act & Assert
        for (int i = 0; i < messages.Length; i++)
        {
            var message = messages[i];
            message.ShouldNotBeNull();
            message.Message.ShouldNotBeNull();
            message.Severity.ShouldBeOneOf(MessageSeverity.Information, MessageSeverity.Warning, MessageSeverity.Error, MessageSeverity.Critical);
            // Output($"Message {i}: {message.Severity} - {message.Message}");
        }
    }

    [Fact]
    public void IFdwMessagePropertiesShouldBeNullableWhereAppropriate()
    {
        // Arrange
        var messageType = typeof(IFdwMessage);

        // Act & Assert
        var codeProperty = messageType.GetProperty("Code")!;
        var sourceProperty = messageType.GetProperty("Source")!;
        var messageProperty = messageType.GetProperty("Message")!;
        var severityProperty = messageType.GetProperty("Severity")!;

        // Code and Source should be nullable (string?)
        var codeIsNullable = Nullable.GetUnderlyingType(codeProperty.PropertyType) != null || 
                            !codeProperty.PropertyType.IsValueType;
        var sourceIsNullable = Nullable.GetUnderlyingType(sourceProperty.PropertyType) != null || 
                              !sourceProperty.PropertyType.IsValueType;

        // Message should be non-nullable (string)
        var messageIsNullable = Nullable.GetUnderlyingType(messageProperty.PropertyType) != null;

        // Severity should be non-nullable (enum)
        var severityIsNullable = Nullable.GetUnderlyingType(severityProperty.PropertyType) != null;

        codeIsNullable.ShouldBeTrue();
        sourceIsNullable.ShouldBeTrue();
        messageIsNullable.ShouldBeFalse();
        severityIsNullable.ShouldBeFalse();

        // Output("Nullable properties are correctly defined");
    }

    // Test implementation of IFdwMessage for testing purposes
    private sealed class TestFdwMessage : IFdwMessage
    {
        public TestFdwMessage(MessageSeverity severity, string message, string? code, string? source)
        {
            Severity = severity;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Code = code;
            Source = source;
        }

        public MessageSeverity Severity { get; }
        public string Message { get; }
        public string? Code { get; }
        public string? Source { get; }
    }
}