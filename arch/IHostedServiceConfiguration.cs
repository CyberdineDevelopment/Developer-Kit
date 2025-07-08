using FractalDataWorks.Net.Interfaces;

namespace FractalDataWorks.Hosts
{
    public interface IHostedServiceConfiguration : IEnhancedConfiguration
    {
        bool Enabled { get; set; }
        int StartupDelayMilliseconds { get; set; }
        int ShutdownTimeoutMilliseconds { get; set; }
    }
}