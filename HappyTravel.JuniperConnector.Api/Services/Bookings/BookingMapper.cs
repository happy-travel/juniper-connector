using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Api.Services.Bookings;

public class BookingMapper
{
    public static Booking Map(JP_Reservation reservation, string referenceCode, DateTimeOffset checkInDate, DateTimeOffset checkOutDate)
    {
        var hotelItem = reservation.Items.HotelItem.Single();

        return new Booking(referenceCode: referenceCode,
            status: GetBookingStatus(),
            accommodationId: hotelItem.HotelInfo.Code,
            supplierReferenceCode: reservation.Locator,
            checkInDate: checkInDate,
            checkOutDate: checkOutDate,
            rooms: GetRooms(),
            bookingUpdateMode: BookingUpdateModes.Synchronous,
            specialValues: new List<KeyValuePair<string, string>>());


        BookingStatusCodes GetBookingStatus()
            => reservation.Status switch
            {
                JP_ResStatus.PAG => BookingStatusCodes.Confirmed,
                JP_ResStatus.CON => BookingStatusCodes.Confirmed,
                JP_ResStatus.CAN => BookingStatusCodes.Cancelled,
                JP_ResStatus.CAC => BookingStatusCodes.Cancelled,
                JP_ResStatus.PRE => BookingStatusCodes.WaitingForResponse,
                JP_ResStatus.PDI => BookingStatusCodes.WaitingForResponse,
                JP_ResStatus.QUO => BookingStatusCodes.WaitingForResponse,
                _ => BookingStatusCodes.NotFound,
            };


        List<SlimRoomOccupation> GetRooms()
        {
            var leader = reservation.Paxes.Pax.Single(p => p.IdPax == reservation.Holder.RelPax.IdPax);

            var paxes = reservation.Paxes.Pax.Select(p => (Id: p.IdPax, Pax: new Pax(title: GetPaxTitle(p.Title),
                    lastName: p.Surname,
                    firstName: p.Name,
                    isLeader: IsLeader(p),
                    age: p.Age)))
                .ToList();

            return reservation.Items.HotelItem.Single().HotelRooms.Select(r => new SlimRoomOccupation(type: RoomTypes.NotSpecified,
                    passengers: r.RelPaxes.Select(p =>
                        paxes.Single(pax => pax.Id == p.IdPax).Pax)
                    .ToList(),
                    supplierRoomReferenceCode: r.Source))
                .ToList();


            PassengerTitles GetPaxTitle(string title)
                => title switch
                {
                    "MR" => PassengerTitles.Mr,
                    "MSTR" => PassengerTitles.Mr,
                    "MRS" => PassengerTitles.Mrs,
                    "MISS" => PassengerTitles.Miss,
                    _ => PassengerTitles.Unspecified
                };


            bool IsLeader(JP_Pax pax)
            {
                if (leader.Name == pax.Name && leader.Surname == pax.Surname
                    && leader.Age == pax.Age && leader.Title == pax.Title)
                    return true;

                return false;
            }
        }
    }
}
