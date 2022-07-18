using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.JuniperConnector.Api.Services;
using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class AvailabilityRequestExtensions
{
    public static int GetNumberOfNights(this AvailabilityRequest request)
       => request.CheckOutDate.Date.Subtract(request.CheckInDate.Date).Days;


    public static JP_HotelAvail ToJuniperAvailabilityRequest(this AvailabilityRequest request, List<string> accommodationIds)
    {
        var (basePaxes, roomPaxes) = GetPaxes();

        return new JP_HotelAvail()
        {          
            Paxes = basePaxes,
            HotelRequest = new JP_RequestHotelsAvail()
            {
                SearchSegmentsHotels = new JP_SearchSegmentsHotels()
                {
                    SearchSegmentHotels = new JP_SearchSegmentHotels()
                    {
                        Start = request.CheckInDate.DateTime,
                        End = request.CheckOutDate.DateTime
                    },
                    HotelCodes = accommodationIds.ToArray(),
                    CountryOfResidence = request.Residency,
                    PaymentType = JP_SearchSegmentsHotelsPaymentType.ExcludePaymentInDestination,
                    PaymentTypeSpecified = true
                },
                RelPaxesDist = roomPaxes
            },
            AdvancedOptions = new JP_HotelAvailAdvancedOptions()
            {
                ShowOnlyAvailable = true,
                ShowOnlyAvailableSpecified = true,
                ShowAllCombinations = true,
                ShowAllCombinationsSpecified = false,
                ShowCancellationPolicies = true,
                ShowCancellationPoliciesSpecified = true,
                TimeOut = Constants.DefaultTimeout,
                TimeOutSpecified = true
            }
        };


        (JP_Paxes, JP_HotelRelPaxDist[]) GetPaxes()
        {
            var basePaxes = new List<JP_Pax>();
            var roomPaxes = new List<JP_HotelRelPaxDist>();
            
            var paxIdsCounter = 1;
            foreach(var room in request.Rooms)
            {
                var roomPaxIds = new List<int>();

                for (int i = 0; i < room.AdultsNumber; i++)
                {
                    basePaxes.Add(new()
                    {
                        IdPax = paxIdsCounter
                    });
                    roomPaxIds.Add(paxIdsCounter);

                    paxIdsCounter++;
                }

                foreach(var childAge in room.ChildrenAges)
                {
                    basePaxes.Add(new()
                    {
                        IdPax = paxIdsCounter,
                        Age = childAge,
                        AgeSpecified=true
                    });
                    roomPaxIds.Add(paxIdsCounter);

                    paxIdsCounter++;
                }

                roomPaxes.Add(new()
                {
                    RelPaxes = roomPaxIds.Select(id =>
                          new JP_RelPax()
                          {
                              IdPax = id
                          })
                    .ToArray()
                });
            }

            return (new JP_Paxes()
                {
                    Pax = basePaxes.ToArray()
                }, 
                roomPaxes.ToArray());
        }
    }
}