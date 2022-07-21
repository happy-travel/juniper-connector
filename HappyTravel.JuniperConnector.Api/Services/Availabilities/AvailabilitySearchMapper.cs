using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;
using HappyTravel.JuniperConnector.Api.Models.Availability;
using HappyTravel.JuniperConnector.Api.Services.Caching;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities;

public class AvailabilitySearchMapper
{
    public AvailabilitySearchMapper(AvailabilitySearchResultStorage availabilitySearchResultStorage)
    {
        _availabilitySearchResultStorage = availabilitySearchResultStorage;
    }


    public async Task<Availability> MapToAvailability(AvailabilityRequest availabilityRequest, List<JP_Results> responses)
    {
        var hotelResults = responses.SelectMany(r => r.Items)
            .Select(hotelResult => (JP_HotelResult)hotelResult)
            .ToList();

        var availabilityId = Guid.NewGuid().ToString();
        var numberOfNights = availabilityRequest.GetNumberOfNights();

        var slimAccommodationAvailabilities = new List<SlimAccommodationAvailability>();
        var cachedAccommodationAvailabilities = new List<CachedAccommodationAvailability>();

        foreach (var hotelResult in hotelResults)
        {
            var slimAccommodationAvailability = new CachedAccommodationAvailability(
                hotelResult.JPCode,
                MapRooms(hotelResult, availabilityRequest, numberOfNights));

            slimAccommodationAvailabilities.Add(slimAccommodationAvailability.ToContract(availabilityId));
            cachedAccommodationAvailabilities.Add(slimAccommodationAvailability);
        }

        await _availabilitySearchResultStorage.Set(availabilityId, cachedAccommodationAvailabilities);

        return new Availability(availabilityId: availabilityId,
            numberOfNights: numberOfNights,
            checkInDate: availabilityRequest.CheckInDate,
            checkOutDate: availabilityRequest.CheckOutDate,
            results: slimAccommodationAvailabilities,
            expiredAfter: DateTimeOffset.MinValue,
            numberOfProcessedAccommodations: hotelResults.Count);
    }


    private List<CachedRoomContractSet> MapRooms(JP_HotelResult hotel, AvailabilityRequest availabilityRequest, int numberOfNights)
    {
        var cachedRoomContractSets = new List<CachedRoomContractSet>();

        foreach (var hotelOption in hotel.HotelOptions)
        {
            var id = Guid.NewGuid();                
            var price = hotelOption.Prices.First(p => p.Type == JP_PriceType.S);
            if (!Enum.TryParse(price.Currency, out Currencies currency))
                throw new NotSupportedException($"Currency '{price.Currency}' is not supported");
            var totalGrossPrice = price.TotalFixAmounts?.Gross ?? 0d;
            var totalNetPrice = price.TotalFixAmounts?.Nett ?? 0d;
            var deadline = hotelOption.NonRefundableSpecified && hotelOption.NonRefundable
                ? new Deadline(DateTimeOffset.UtcNow, new List<CancellationPolicy> { new(DateTimeOffset.UtcNow, 100) })
                : DeadlineMapper.GetDeadline(hotelOption.CancellationPolicy, availabilityRequest.CheckInDate, totalGrossPrice, numberOfNights);
            var roomCombination = GetRoomCombination(hotelOption.HotelRooms);
            var roomsCount = hotelOption.HotelRooms.Count();
            var (roomGrossPrice, roomNetPrice, lastRoomGrossPrice, lastRoomNetPrice) = GetRoomPrices(Convert.ToDecimal(totalGrossPrice), Convert.ToDecimal(totalNetPrice), roomsCount, currency);

            cachedRoomContractSets.Add(new CachedRoomContractSet(RatePlanCode: hotelOption.RatePlanCode,
                Id: id,
                Rate: new Rate(new MoneyAmount((decimal)totalNetPrice, currency),
                    new MoneyAmount((decimal)totalGrossPrice, currency)),
                Deadline: deadline,
                Rooms: roomCombination
                    .Select((r, i) => CreateRoomContract(
                        room: r,
                        board: hotelOption.Board,
                        rate: new Rate(i != roomsCount - 1
                                ? roomGrossPrice
                                : lastRoomGrossPrice,
                                i != roomsCount - 1
                                ? roomNetPrice
                                : lastRoomNetPrice),
                        adultsNumber: availabilityRequest.Rooms[i].AdultsNumber,
                        childrenAges: availabilityRequest.Rooms[i].ChildrenAges,
                        deadline: deadline))
                    .ToList(),
                Tags: new List<string>(),
                IsDirectContract: false,
                IsAdvancePurchaseRate: false,
                IsPackageRate: false));
        }

        return cachedRoomContractSets;


        (MoneyAmount, MoneyAmount, MoneyAmount, MoneyAmount) GetRoomPrices(decimal totalGrossPrice, decimal totalNetPrice, int roomsCount, Currencies currency)
        {
            var roomGrossPrice = MoneyRounder.Ceil((totalGrossPrice / roomsCount).ToMoneyAmount(currency));
            var roomNetPrice = MoneyRounder.Ceil((totalNetPrice / roomsCount), currency).ToMoneyAmount(currency);

            return (roomGrossPrice,
                roomNetPrice,
                MoneyRounder.Ceil((totalGrossPrice - (roomGrossPrice.Amount * (roomsCount - 1))).ToMoneyAmount(currency)),
                MoneyRounder.Ceil((totalNetPrice - (roomNetPrice.Amount * (roomsCount - 1))).ToMoneyAmount(currency)));
        }


        List<JP_HotelRoom> GetRoomCombination(JP_HotelRoom[] apiRooms)
            => availabilityRequest.Rooms
                .Select((r, i) => apiRooms
                    .Single(apiRoom => apiRoom.Source.Split(',').Contains((i + 1).ToString()))) // "i + 1" because "Source" is the Room identifier. Starts from "1"
                .ToList();
    }


    private RoomContract CreateRoomContract(JP_HotelRoom room, JP_Board board, Rate rate, int adultsNumber, List<int> childrenAges, Deadline deadline)
    {
        return new RoomContract(
            boardBasis: BoardBasisTypes.NotSpecified,
            mealPlan: board?.Value,
            contractTypeCode: default,
            isAvailableImmediately: true,
            isDynamic: false,
            contractDescription: room.Description ?? room.Name ?? string.Empty,
            remarks: new List<KeyValuePair<string, string>>(),
            dailyRoomRates: new List<DailyRate>(),
            rate: rate,
            adultsNumber: adultsNumber,
            childrenAges: childrenAges,
            type: RoomTypes.NotSpecified,
            isExtraBedNeeded: false,
            deadline: deadline,
            isAdvancePurchaseRate: false);
    }


    private readonly AvailabilitySearchResultStorage _availabilitySearchResultStorage;
}
