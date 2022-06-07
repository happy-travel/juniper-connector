using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(30001, LogLevel.Debug, "WithinearthShoppingClient: {message}")]
    static partial void WithinearthRequestResult(ILogger logger, string message);
    
    [LoggerMessage(30002, LogLevel.Warning, "Get availability request by id `{AvailabilityId}` from storage failed")]
    static partial void GetAvailabilityRequestFromStorageFailed(ILogger logger, string AvailabilityId);
    
    [LoggerMessage(30003, LogLevel.Warning, "Get accommodation by availabilityId `{AvailabilityId}` from storage failed")]
    static partial void GetAccommodationFromStorageFailed(ILogger logger, string AvailabilityId);
    
    [LoggerMessage(30004, LogLevel.Warning, "Get BookingCode by availabilityId `{AvailabilityId}` and room contract set id `{RoomContractSetId}` from storage failed")]
    static partial void GetBookingCodeFromStorageFailed(ILogger logger, string AvailabilityId, System.Guid RoomContractSetId);
    
    
    
    public static void LogWithinearthRequestResult(this ILogger logger, string message)
        => WithinearthRequestResult(logger, message);
    
    public static void LogGetAvailabilityRequestFromStorageFailed(this ILogger logger, string AvailabilityId)
        => GetAvailabilityRequestFromStorageFailed(logger, AvailabilityId);
    
    public static void LogGetAccommodationFromStorageFailed(this ILogger logger, string AvailabilityId)
        => GetAccommodationFromStorageFailed(logger, AvailabilityId);
    
    public static void LogGetBookingCodeFromStorageFailed(this ILogger logger, string AvailabilityId, System.Guid RoomContractSetId)
        => GetBookingCodeFromStorageFailed(logger, AvailabilityId, RoomContractSetId);
}