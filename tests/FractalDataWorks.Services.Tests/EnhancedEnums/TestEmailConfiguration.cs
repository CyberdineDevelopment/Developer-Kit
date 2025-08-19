using System;

namespace FractalDataWorks.Services.Tests.EnhancedEnums;

public sealed class TestEmailConfiguration : IFdwConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public string SectionName { get; set; } = string.Empty;

    public bool Validate() => !string.IsNullOrEmpty(Name);
}