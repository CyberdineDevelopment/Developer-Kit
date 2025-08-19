using FractalDataWorks.Services.EnhancedEnums;

namespace FractalDataWorks.Services.Tests.EnhancedEnums;

public sealed class TestEmailServiceTypeCustomSection : ServiceTypeOptionBase<TestEmailServiceTypeCustomSection, ITestEmailService, TestEmailConfiguration, TestEmailServiceFactory>
{
    public static readonly TestEmailServiceTypeCustomSection CustomSection = new(1, "CustomSection");

    private TestEmailServiceTypeCustomSection(int id, string name) : base(id, name)
    {
    }

    protected override string ConfigurationSection => "CustomEmail:Configurations";
}