using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.JuniperConnector.Updater.Infrastructure.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(40001, LogLevel.Information, "Started worker '{workerName}'")]
    static partial void StartedWorker(ILogger logger, string workerName);
    
    [LoggerMessage(40002, LogLevel.Information, "Finished worker '{workerName}'")]
    static partial void FinishedWorker(ILogger logger, string workerName);
    
    [LoggerMessage(40003, LogLevel.Error, "Cancelling operation '{operationName}'")]
    static partial void CancellingOperation(ILogger logger, string operationName);
    
    [LoggerMessage(40004, LogLevel.Error, "Updater: ")]
    static partial void StaticDataUpdateHostedServiceException(ILogger logger, System.Exception exception);
    
    [LoggerMessage(40005, LogLevel.Information, "Stopping operation '{operationName}'")]
    static partial void StoppingOperation(ILogger logger, string operationName);
    
    [LoggerMessage(40010, LogLevel.Information, "Starting raw data zone update")]
    static partial void StartingZonesUpdate(ILogger logger);
    
    [LoggerMessage(40011, LogLevel.Information, "Finish raw data zone update")]
    static partial void FinishZonesUpdate(ILogger logger);
    
    [LoggerMessage(40012, LogLevel.Error, "ZoneList request failed")]
    static partial void ZoneListRequestFailed(ILogger logger, System.Exception exception);
    
    [LoggerMessage(40020, LogLevel.Information, "Starting raw data hotels update")]
    static partial void StartingHotelsUpdate(ILogger logger);
    
    [LoggerMessage(40021, LogLevel.Information, "Finish raw data update")]
    static partial void FinishHotelsUpdate(ILogger logger);
    
    [LoggerMessage(40022, LogLevel.Information, "Deactivate all hotels")]
    static partial void DeactivateAllHotels(ILogger logger);
    
    [LoggerMessage(40023, LogLevel.Error, "HotelPortfolio request failed")]
    static partial void HotelPortfolioRequestFailed(ILogger logger, System.Exception exception);
    
    [LoggerMessage(40024, LogLevel.Error, "HotelContent request failed")]
    static partial void HotelContentRequestFailed(ILogger logger, System.Exception exception);
    
    
    
    public static void LogStartedWorker(this ILogger logger, string workerName)
        => StartedWorker(logger, workerName);
    
    public static void LogFinishedWorker(this ILogger logger, string workerName)
        => FinishedWorker(logger, workerName);
    
    public static void LogCancellingOperation(this ILogger logger, string operationName)
        => CancellingOperation(logger, operationName);
    
    public static void LogStaticDataUpdateHostedServiceException(this ILogger logger, System.Exception exception)
        => StaticDataUpdateHostedServiceException(logger, exception);
    
    public static void LogStoppingOperation(this ILogger logger, string operationName)
        => StoppingOperation(logger, operationName);
    
    public static void LogStartingZonesUpdate(this ILogger logger)
        => StartingZonesUpdate(logger);
    
    public static void LogFinishZonesUpdate(this ILogger logger)
        => FinishZonesUpdate(logger);
    
    public static void LogZoneListRequestFailed(this ILogger logger, System.Exception exception)
        => ZoneListRequestFailed(logger, exception);
    
    public static void LogStartingHotelsUpdate(this ILogger logger)
        => StartingHotelsUpdate(logger);
    
    public static void LogFinishHotelsUpdate(this ILogger logger)
        => FinishHotelsUpdate(logger);
    
    public static void LogDeactivateAllHotels(this ILogger logger)
        => DeactivateAllHotels(logger);
    
    public static void LogHotelPortfolioRequestFailed(this ILogger logger, System.Exception exception)
        => HotelPortfolioRequestFailed(logger, exception);
    
    public static void LogHotelContentRequestFailed(this ILogger logger, System.Exception exception)
        => HotelContentRequestFailed(logger, exception);
}