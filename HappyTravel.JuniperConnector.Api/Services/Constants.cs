namespace HappyTravel.JuniperConnector.Api.Services;

public static class Constants
{
    public static readonly TimeSpan StepCacheLifeTime = TimeSpan.FromMinutes(5);

    public const int DefaultAdultAge = 30;

    public const int DefaultTimeout = 8000; // In responses, a timeout of 8 ms is recommended.
}
