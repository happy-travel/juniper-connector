using NetTopologySuite.Geometries;

namespace HappyTravel.JuniperConnector.Data.Models;

public class Accommodation
{
    public string Code { get; set; } = string.Empty;
    public string Country { get; set; }
    public string? Locality { get; set; }
    public Point Coordinates { get; set; } = new Point(0, 0);
    public string Name { get; set; }
    public DateTimeOffset Modified { get; set; }
}
