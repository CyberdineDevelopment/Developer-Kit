using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace FractalDataWorks.Hosts
{
    public abstract class BaseHost
    {
        protected IHostBuilder? _hostBuilder;
        protected IHost? _host;
        protected ILogger? _bootstrapLogger;

        protected BaseHost()
        {
            ConfigureBootstrapLogger();
        }

        protected virtual void ConfigureBootstrapLogger()
        {
            _bootstrapLogger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateBootstrapLogger();

            Log.Logger = _bootstrapLogger;
        }

        public virtual IHostBuilder CreateHostBuilder(string[] args)
        {
            _hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);

                    ConfigureAdditionalConfiguration(hostContext, config);
                })
                .UseSerilog((context, services, configuration) =>
                {
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .Enrich.WithEnvironmentName()
                        .Enrich.WithMachineName()
                        .Enrich.WithProcessId()
                        .Enrich.WithThreadId();

                    ConfigureSerilog(context, services, configuration);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    ConfigureServices(hostContext, services);
                });

            return _hostBuilder;
        }

        protected virtual void ConfigureAdditionalConfiguration(HostBuilderContext hostContext, IConfigurationBuilder config)
        {
        }

        protected virtual void ConfigureSerilog(HostBuilderContext context, IServiceProvider services, LoggerConfiguration configuration)
        {
        }

        protected abstract void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services);

        public virtual void Run(string[] args)
        {
            try
            {
                Log.Information("Starting host {HostType}", GetType().Name);
                _host = CreateHostBuilder(args).Build();
                _host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public virtual async Task RunAsync(string[] args)
        {
            try
            {
                Log.Information("Starting host {HostType}", GetType().Name);
                _host = CreateHostBuilder(args).Build();
                await _host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}