using FractalDataWorks.Services.EnhancedEnums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.Tests.EnhancedEnums;

public sealed class TestEmailServiceTypeWithAdditional : ServiceTypeOptionBase<TestEmailServiceTypeWithAdditional, ITestEmailService, TestEmailConfiguration, TestEmailServiceFactory>
{
    public static readonly TestEmailServiceTypeWithAdditional Custom = new(1, "Custom");

    private TestEmailServiceTypeWithAdditional(int id, string name) : base(id, name)
    {
    }

    protected override void RegisterAdditional(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAdditionalTestService, AdditionalTestService>();
    }
}