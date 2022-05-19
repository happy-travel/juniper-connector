namespace HappyTravel.JuniperConnector.Data.Models;

public class StaticDataUpdateHistoryEntry
{
    public int Id { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? FinishTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string Options { get; set; }
}
