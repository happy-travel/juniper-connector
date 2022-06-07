using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.JuniperConnector.Api.Services.Availabilities;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class RoomContractSetExtensions
{
    public static RoomContractSet Update(this RoomContractSet roomContractSet, JP_CancellationPolicyRules cancelPolicy, JP_Price price, DateTimeOffset checkInDate, 
        int numberOfNights, JP_Comment[] comments)
    {
        var deadline = DeadlineMapper.GetDeadline(cancelPolicy, checkInDate, price.TotalFixAmounts.Gross, numberOfNights);
        var netDifference = roomContractSet.Rate.FinalPrice.Amount / Convert.ToDecimal(price.TotalFixAmounts.Nett);
        var grossDifference = roomContractSet.Rate.Gross.Amount / Convert.ToDecimal(price.TotalFixAmounts.Gross);

        return new RoomContractSet(id: roomContractSet.Id,
            rate: roomContractSet.Rate.SetPrice(price),
            deadline: deadline,
            rooms: roomContractSet.RoomContracts.Update(deadline, netDifference, grossDifference, comments),
            tags: roomContractSet.Tags,
            isDirectContract: roomContractSet.IsDirectContract,
            isAdvancePurchaseRate: roomContractSet.IsAdvancePurchaseRate,
            isPackageRate: roomContractSet.IsPackageRate);
    }


    // Juniper api doesnt return prices for rooms, only total price
    // We change the price proportionally so that the sum converges
    private static Rate ChangeForTheDifference(this Rate rate, decimal netDifference, decimal grossDifference)
        => new(finalPrice: MoneyRounder.Ceil((rate.FinalPrice.Amount * netDifference).ToMoneyAmount(rate.FinalPrice.Currency)),
            gross: MoneyRounder.Ceil((rate.Gross.Amount * grossDifference).ToMoneyAmount(rate.Gross.Currency)),
            discounts: rate.Discounts,
            type: rate.Type,
            description: rate.Description);


    private static Rate SetPrice(this Rate rate, JP_Price price)
    {
        if (!Enum.TryParse(price.Currency, out Currencies currency))
            throw new NotSupportedException($"Currency '{price.Currency}' is not supported");

        return new(finalPrice: new MoneyAmount((decimal)price.TotalFixAmounts.Nett, currency),
            gross: new MoneyAmount((decimal)price.TotalFixAmounts.Gross, currency),
            discounts: rate.Discounts,
            type: rate.Type,
            description: rate.Description);
    }


    private static List<RoomContract> Update(this List<RoomContract> roomContracts, Deadline deadline, decimal netDifference, decimal grossDifference, JP_Comment[] comments)
    {
        return roomContracts.Select(r => new RoomContract(boardBasis: r.BoardBasis,
            mealPlan: r.MealPlan,
            contractTypeCode: r.ContractTypeCode,
            isAvailableImmediately: r.IsAvailableImmediately,
            isDynamic: r.IsDynamic,
            contractDescription: r.ContractDescription,
            remarks: GetRemarks(r.Remarks),
            dailyRoomRates: r.DailyRoomRates,
            rate: r.Rate.ChangeForTheDifference(netDifference, grossDifference),
            adultsNumber: r.AdultsNumber,
            childrenAges: r.ChildrenAges,
            type: r.Type,
            isExtraBedNeeded: r.IsExtraBedNeeded,
            deadline: deadline,
            isAdvancePurchaseRate: r.IsAdvancePurchaseRate))
            .ToList();


        List<KeyValuePair<string, string>> GetRemarks(List<KeyValuePair<string, string>> remarks)
        {
            var allRemarks = new List<KeyValuePair<string, string>>();

            allRemarks.AddRange(remarks);

            if (comments.Count() > 0)
                allRemarks.AddRange(comments.Select(c =>
                    new KeyValuePair<string, string>(c.Type.ToString(), c.Value))
                    .ToList());

            return allRemarks;
        }
    }
}
