namespace HappyTravel.JuniperConnector.Updater.Workers;

public interface IUpdateWorker
{
    Task Run(CancellationToken cancellationToken);
}
