namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class TaskExtensions
{
    /// <summary>
    /// Waiting for tasks to complete
    /// </summary>
    /// <param name="tasks">Task list</param>
    /// <returns></returns>
    public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);
}