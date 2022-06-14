using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class BookingRequestExtensions
{
    public static JP_HotelBooking ToJuniperBookingRequest(this BookingRequest request, string bookingCode, string residency, string accommodationId, 
        DateTimeOffset checkInDate, DateTimeOffset checkOutDate, double price, Currencies currency)
    {
        var paxes = GetPaxes();

        return new JP_HotelBooking()
        {
            Paxes = new JP_Paxes()
            {
                Pax = paxes.SelectMany(p => p)
                    .Select(p => new JP_Pax()
                    {
                        IdPax = p.Item1,
                        Title = p.Item2.GetPaxTitle(),
                        Name = p.Item2.FirstName,
                        Surname = p.Item2.LastName,
                        Age = p.Item2.Age,
                        AgeSpecified = true,                        
                        Nationality = residency
                    })
                    .ToArray()
            },
            ExternalBookingReference = request.ReferenceCode,
            Holder = new JP_Holder()
            {
                RelPax = new JP_RelPax()
                {
                    IdPax = paxes.SelectMany(p => p)
                        .Single(p => p.Item2.IsLeader).Item1
                }
            },
            Elements = new JP_HotelElement[]
            {
                new JP_HotelElement()
                {
                    BookingCode = new JP_BookingCode()
                    {
                         Value = bookingCode
                    },
                    RelPaxesDist = paxes.Select(roomPaxes => new JP_RelPaxDist()
                    {
                        RelPaxes = roomPaxes.Select(p=>new JP_RelPax()
                        {
                            IdPax = p.Item1
                        })
                        .ToArray()
                    })
                    .ToArray(),
                    CreditCard = GetCreditCardInfo(request.CreditCard),
                    HotelBookingInfo = new JP_HotelBookingInfo()
                    {
                        Start = checkInDate.DateTime,
                        StartSpecified = true,
                        End = checkOutDate.DateTime,
                        EndSpecified = true,
                        HotelCode = accommodationId,
                        Status = JP_AvailStatus.OK,
                        StatusSpecified = true,
                        Price = new JP_BookingPrice()
                        {
                            PriceRange = new JP_PriceRange()
                            {
                                Currency = currency.ToString(),
                                Minimum = price,
                                Maximum = price
                            }
                        }
                    }
                }
            }
        };

        
        List<List<(int, Pax)>> GetPaxes()
        {
            var roomPaxes = new List<List<(int, Pax)>>();
            var paxId = 1;

            foreach (var room in request.Rooms)
            {
                var paxes = new List<(int, Pax)>();

                foreach (var pax in room.Passengers)
                {
                    paxes.Add((paxId, pax));
                    paxId++;
                }

                roomPaxes.Add(paxes);
            }

            return roomPaxes;
        }


        JP_CreditCardInfo? GetCreditCardInfo(CreditCard? vcc)
        {
            if (request.CreditCard is null)
                return default;

            return new JP_CreditCardInfo()
            {
                CardCode = vcc.GetCardCode(),
                CvC = vcc?.Code,
                CardNumber = vcc?.Number,
                Name = vcc?.Holder,
                Surname = string.Empty
            };
        }
    }


    private static string GetPaxTitle(this Pax pax)
        => pax.Title switch
        {
            PassengerTitles.Mr => "MR",
            PassengerTitles.Mrs => "MRS",
            PassengerTitles.Miss => "MISS",
            _ => string.Empty
        };


    private static JP_CreditCardType GetCardCode(this CreditCard? vcc)
        => vcc.Value.CardVendor switch
        {
            CardVendor.AmericanExpress => JP_CreditCardType.AX,
            CardVendor.Visa => JP_CreditCardType.VI,
            CardVendor.MasterCard => JP_CreditCardType.MC,
            CardVendor.Discover => JP_CreditCardType.DS,
            _ => JP_CreditCardType.OT
        };
}
