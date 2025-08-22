using System;
using FluentValidation.Results;
using FractalDataWorks;
using FractalDataWorks.Services.Scheduling.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests;

public sealed class IScheduleCommandTests
{
    [Fact]
    public void ScheduleCommandShouldInheritFromICommand()
    {
        // Arrange & Act
        var scheduleCommandType = typeof(IScheduleCommand);
        
        // Assert
        scheduleCommandType.IsAssignableFrom(typeof(ICommand)).ShouldBeTrue();
        scheduleCommandType.IsInterface.ShouldBeTrue();
        scheduleCommandType.Name.ShouldBe("IScheduleCommand");
        scheduleCommandType.Namespace.ShouldBe("FractalDataWorks.Services.Scheduling.Abstractions");
    }

    [Fact]
    public void ScheduleCommandShouldHaveCorrectHierarchy()
    {
        // Arrange & Act
        var scheduleCommandType = typeof(IScheduleCommand);
        var interfaces = scheduleCommandType.GetInterfaces();
        
        // Assert
        interfaces.ShouldContain(typeof(ICommand));
        scheduleCommandType.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public void ScheduleCommandImplementationShouldFollowContract()
    {
        // Arrange
        var implementation = new TestScheduleCommand();
        
        // Act & Assert
        (implementation is IScheduleCommand).ShouldBeTrue();
        (implementation is ICommand).ShouldBeTrue();
        implementation.CommandId.ShouldNotBe(Guid.Empty);
        implementation.CorrelationId.ShouldNotBe(Guid.Empty);
        implementation.Timestamp.ShouldNotBe(DateTimeOffset.MinValue);
    }

    [Fact]
    public void ScheduleCommandShouldBeSerializable()
    {
        // Arrange
        var implementation = new TestScheduleCommand();
        
        // Act & Assert  
        // Modern approach doesn't require Serializable attribute - test JSON serialization instead
        // But should be JSON serializable
        var json = System.Text.Json.JsonSerializer.Serialize(implementation);
        json.ShouldNotBeEmpty();
        
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<TestScheduleCommand>(json);
        deserialized.ShouldNotBeNull();
    }

    private sealed class TestScheduleCommand : IScheduleCommand
    {
        public Guid CommandId { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
        public IFdwConfiguration? Configuration { get; set; }

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }
}