using HappyTravel.JuniperConnector.Updater.Infrastructure.Logging;
using HappyTravel.JuniperConnector.Updater.Workers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.JuniperConnector.Updater;

public class StaticDataUpdateHostedService : BackgroundService
{
    public StaticDataUpdateHostedService(IEnumerable<IUpdateWorker> updateWorkers, IHostApplicationLifetime applicationLifetime, ILogger<StaticDataUpdateHostedService> logger)
    {
        _updateWorkers = updateWorkers;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            stoppingToken.ThrowIfCancellationRequested();

            foreach (var worker in _updateWorkers)
            {
                var workerName = worker.GetType().Name;
                stoppingToken.ThrowIfCancellationRequested();
                _logger.LogStartedWorker(workerName);

                await worker.Run(stoppingToken);

                _logger.LogFinishedWorker(workerName);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogCancellingOperation(nameof(StaticDataUpdateHostedService));
        }
        catch (Exception ex)
        {
            _logger.LogStaticDataUpdateHostedServiceException(ex);
        }

        _logger.LogStoppingOperation(nameof(StaticDataUpdateHostedService));
        _applicationLifetime.StopApplication();
    }


    private readonly IEnumerable<IUpdateWorker> _updateWorkers;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<StaticDataUpdateHostedService> _logger;
}
