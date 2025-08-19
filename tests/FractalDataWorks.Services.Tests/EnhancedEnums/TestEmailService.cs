using System.Threading.Tasks;

namespace FractalDataWorks.Services.Tests.EnhancedEnums;

public sealed class TestEmailService : ITestEmailService
{
    private readonly TestEmailConfiguration _configuration;

    public TestEmailService(TestEmailConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Id => _configuration.Id.ToString();
    public string ServiceType => _configuration.Name;
    public bool IsAvailable => _configuration.IsEnabled;

    public Task<IFdwResult> SendEmailAsync(string to, string subject, string body)
    {
        return Task.FromResult(FdwResult.Success());
    }
}