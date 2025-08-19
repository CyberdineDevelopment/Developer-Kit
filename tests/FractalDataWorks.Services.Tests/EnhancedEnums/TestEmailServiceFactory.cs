using System;
using System.Threading.Tasks;

namespace FractalDataWorks.Services.Tests.EnhancedEnums;

public sealed class TestEmailServiceFactory : ServiceFactoryBase<ITestEmailService, TestEmailConfiguration>
{
    public TestEmailServiceFactory() : base(null)
    {
    }

    protected override IFdwResult<ITestEmailService> CreateCore(TestEmailConfiguration configuration)
    {
        var service = new TestEmailService(configuration);
        return FdwResult<ITestEmailService>.Success(service);
    }

    public override Task<ITestEmailService> GetService(string configurationName)
    {
        var config = new TestEmailConfiguration { Name = configurationName };
        var result = CreateCore(config);
        return Task.FromResult(result.IsSuccess ? result.Value! : throw new InvalidOperationException(result.Message));
    }

    public override Task<ITestEmailService> GetService(int configurationId)
    {
        var config = new TestEmailConfiguration { Id = configurationId, Name = "Test" };
        var result = CreateCore(config);
        return Task.FromResult(result.IsSuccess ? result.Value! : throw new InvalidOperationException(result.Message));
    }
}