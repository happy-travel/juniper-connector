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
    
    [LoggerMessage(40006, LogLevel.Information, "Starting raw data zone update")]
    static partial void StartingZonesUpdate(ILogger logger);
    
    [LoggerMessage(40007, LogLevel.Critical, "Zone loader: ")]
    static partial void ZoneLoaderException(ILogger logger, System.Exception exception);
    
    [LoggerMessage(40008, LogLevel.Information, "Starting raw data hotels update with id '{updateId}'")]
    static partial void StartingHotelsUpdate(ILogger logger, int updateId);
    
    [LoggerMessage(40009, LogLevel.Information, "Deactivate all hotels")]
    static partial void DeactivateAllHotels(ILogger logger);
    
    [LoggerMessage(40010, LogLevel.Critical, "Hotels loader: ")]
    static partial void HotelLoaderException(ILogger logger, System.Exception exception);
    
    [LoggerMessage(40011, LogLevel.Information, "Finish raw data update with id '{updateId}'")]
    static partial void FinishHotelsUpdate(ILogger logger, int updateId);
    
    
    
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
    
    public static void LogZoneLoaderException(this ILogger logger, System.Exception exception)
        => ZoneLoaderException(logger, exception);
    
    public static void LogStartingHotelsUpdate(this ILogger logger, int updateId)
        => StartingHotelsUpdate(logger, updateId);
    
    public static void LogDeactivateAllHotels(this ILogger logger)
        => DeactivateAllHotels(logger);
    
    public static void LogHotelLoaderException(this ILogger logger, System.Exception exception)
        => HotelLoaderException(logger, exception);
    
    public static void LogFinishHotelsUpdate(this ILogger logger, int updateId)
        => FinishHotelsUpdate(logger, updateId);
}