using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(30001, LogLevel.Error, "Search request failed")]
    static partial void SearchRequestFailed(ILogger logger, System.Exception exception);
    
    [LoggerMessage(30002, LogLevel.Error, "BookingRules request failed")]
    static partial void BookingRulesRequestFailed(ILogger logger, System.Exception exception);
    
    [LoggerMessage(30010, LogLevel.Warning, "Get availability request by id `{AvailabilityId}` from storage failed")]
    static partial void GetAvailabilityRequestFromStorageFailed(ILogger logger, string AvailabilityId);
    
    [LoggerMessage(30011, LogLevel.Warning, "Get accommodation by availabilityId `{AvailabilityId}` from storage failed")]
    static partial void GetAccommodationFromStorageFailed(ILogger logger, string AvailabilityId);
    
    [LoggerMessage(30004, LogLevel.Warning, "Get BookingCode by availabilityId `{AvailabilityId}` and room contract set id `{RoomContractSetId}` from storage failed")]
    static partial void GetBookingCodeFromStorageFailed(ILogger logger, string AvailabilityId, System.Guid RoomContractSetId);
    
    
    
    public static void LogSearchRequestFailed(this ILogger logger, System.Exception exception)
        => SearchRequestFailed(logger, exception);
    
    public static void LogBookingRulesRequestFailed(this ILogger logger, System.Exception exception)
        => BookingRulesRequestFailed(logger, exception);
    
    public static void LogGetAvailabilityRequestFromStorageFailed(this ILogger logger, string AvailabilityId)
        => GetAvailabilityRequestFromStorageFailed(logger, AvailabilityId);
    
    public static void LogGetAccommodationFromStorageFailed(this ILogger logger, string AvailabilityId)
        => GetAccommodationFromStorageFailed(logger, AvailabilityId);
    
    public static void LogGetBookingCodeFromStorageFailed(this ILogger logger, string AvailabilityId, System.Guid RoomContractSetId)
        => GetBookingCodeFromStorageFailed(logger, AvailabilityId, RoomContractSetId);
}