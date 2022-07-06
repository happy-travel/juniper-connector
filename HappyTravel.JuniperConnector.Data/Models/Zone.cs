namespace HappyTravel.JuniperConnector.Data.Models;

public class Zone
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string IATA { get; set; }
    public string ParentCode { get; set; }
    public ZoneType AreaType { get; set; }
}
