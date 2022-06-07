namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class TaskExtensions
{
    public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);
}