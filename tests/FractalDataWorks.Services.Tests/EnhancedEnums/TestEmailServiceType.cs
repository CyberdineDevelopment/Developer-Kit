using FractalDataWorks.Services.EnhancedEnums;

namespace FractalDataWorks.Services.Tests.EnhancedEnums;

public sealed class TestEmailServiceType : ServiceTypeOptionBase<TestEmailServiceType, ITestEmailService, TestEmailConfiguration, TestEmailServiceFactory>
{
    public static readonly TestEmailServiceType SendGrid = new(1, "SendGrid");
    public static readonly TestEmailServiceType Smtp = new(2, "SMTP");

    private TestEmailServiceType(int id, string name) : base(id, name)
    {
    }
}