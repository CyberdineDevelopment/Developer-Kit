using System.Threading.Tasks;

namespace FractalDataWorks.Services.Tests.EnhancedEnums;

public interface ITestEmailService : IFdwService
{
    Task<IFdwResult> SendEmailAsync(string to, string subject, string body);
}