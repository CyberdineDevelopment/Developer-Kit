using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.Hosts
{
    public abstract class BaseHostedService<TConfiguration> : IHostedService, IDisposable
        where TConfiguration : IHostedServiceConfiguration
    {
        protected readonly ILogger<BaseHostedService<TConfiguration>> _logger;
        protected readonly TConfiguration _configuration;
        private Timer? _timer;
        private CancellationTokenSource? _stoppingCts;

        protected BaseHostedService(ILogger<BaseHostedService<TConfiguration>> logger, TConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting hosted service {ServiceName}", GetType().Name);

            if (!_configuration.Enabled)
            {
                _logger.LogInformation("Hosted service {ServiceName} is disabled", GetType().Name);
                return;
            }

            _stoppingCts = new CancellationTokenSource();

            if (_configuration.StartupDelayMilliseconds > 0)
            {
                _logger.LogInformation("Delaying startup by {Delay}ms", _configuration.StartupDelayMilliseconds);
                await Task.Delay(_configuration.StartupDelayMilliseconds, cancellationToken);
            }

            await StartServiceAsync(_stoppingCts.Token);
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping hosted service {ServiceName}", GetType().Name);

            if (!_configuration.Enabled)
            {
                return;
            }

            if (_stoppingCts != null)
            {
                try
                {
                    _stoppingCts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            var timeoutCts = new CancellationTokenSource(_configuration.ShutdownTimeoutMilliseconds);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await StopServiceAsync(linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                _logger.LogWarning("Hosted service {ServiceName} shutdown timed out after {Timeout}ms", 
                    GetType().Name, _configuration.ShutdownTimeoutMilliseconds);
            }
        }

        protected abstract Task StartServiceAsync(CancellationToken cancellationToken);
        protected abstract Task StopServiceAsync(CancellationToken cancellationToken);

        public virtual void Dispose()
        {
            _timer?.Dispose();
            _stoppingCts?.Dispose();
        }

        protected void StartTimer(TimeSpan interval, Func<Task> callback)
        {
            _timer = new Timer(async _ => await ExecuteTimerCallback(callback), null, interval, interval);
        }

        private async Task ExecuteTimerCallback(Func<Task> callback)
        {
            try
            {
                await callback();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in timer callback for {ServiceName}", GetType().Name);
            }
        }
    }
}