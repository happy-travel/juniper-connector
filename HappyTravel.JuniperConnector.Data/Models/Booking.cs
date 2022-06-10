namespace HappyTravel.JuniperConnector.Data.Models;

public class Booking
{
    public string ReferenceCode { get; set; }
    public string SupplierReferenceCode { get; set; }
    public DateTimeOffset CheckInDate { get; set; }
    public DateTimeOffset CheckOutDate { get; set; }
}