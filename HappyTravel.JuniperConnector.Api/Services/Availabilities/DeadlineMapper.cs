using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities;

public static class DeadlineMapper
{
    /// <summary>
    /// Deadline mapping.
    /// </summary>
    /// <param name="cancelPolicy">Supplier Cancellation Policy</param>
    /// <param name="checkInDate">Check in date</param>
    /// <param name="totalGrossPrice">Total gross price</param>
    /// <param name="numberOfNights">Number of nights</param>
    /// <returns></returns>
    public static Deadline GetDeadline(JP_CancellationPolicyRules cancelPolicy, DateTimeOffset checkInDate, double totalGrossPrice, int numberOfNights)
    {
        if (cancelPolicy is null)
            return CreateNonRefundableDeadline();

        if (cancelPolicy.PolicyRules.Count() == 0)
            return CreateNonRefundableDeadline(cancelPolicy.Description);

        var polities = new List<CancellationPolicy>();
        foreach (var rule in cancelPolicy.PolicyRules)
        {
            var fromDate = GetFromDateValue(rule);
            var percentage = GetPercentage(rule);

            if (fromDate is not null && percentage is not null && percentage > 0)
                polities.Add(new CancellationPolicy(fromDate.Value, percentage.Value));
        }

        if (polities.Count == 0)
            return CreateNonRefundableDeadline();

        polities = polities.OrderBy(p => p.FromDate).ToList();

        var firstDeadline = GetFirstDeadline(cancelPolicy.FirstDayCostCancellation, polities);

        List<string>? remarks = null;
        if (!string.IsNullOrWhiteSpace(cancelPolicy.Description))
            remarks = new List<string>() { cancelPolicy.Description };

        return new Deadline(firstDeadline, polities, remarks);


        Deadline CreateNonRefundableDeadline(string? description = null)
        {
            if (!string.IsNullOrWhiteSpace(description))
                return new Deadline(date: default,
                    remarks: new List<string>() { description });

            return new Deadline(DateTimeOffset.UtcNow, new List<CancellationPolicy> { new(DateTimeOffset.UtcNow, 100) });
        }


        DateTimeOffset? GetFromDateValue(JP_Rule rule)
        {
            DateTimeOffset? fromDate = null;

            if (!rule.DateFromSpecified && !rule.FromSpecified)
                return default;

            if (rule.DateFromSpecified)
                fromDate = new DateTimeOffset(rule.DateFrom);
            else
            if (rule.FromSpecified)
                fromDate = checkInDate.AddDays(-rule.From);

            if (fromDate is null)
                return default;

            if (!string.IsNullOrWhiteSpace(rule.DateFromHour))
            {
                var dateFromHour = TimeOnly.Parse(rule.DateFromHour); // Indicates the time of the day referred by the DateFrom value. Format: hh:mm
                fromDate.Value.AddHours(dateFromHour.Hour);
                fromDate.Value.AddMinutes(dateFromHour.Minute);
            }

            return fromDate;
        }


        double? GetPercentage(JP_Rule rule)
        {
            if (rule.PercentPriceSpecified)
                return rule.PercentPrice;

            if (rule.FixedPriceSpecified)
                return Math.Round(100 * rule.FixedPrice / totalGrossPrice, 2);

            if (rule.NightsSpecified)
            {
                if (rule.MostExpensiveNightPriceSpecified)
                    return Math.Round(100 * rule.MostExpensiveNightPrice * rule.Nights / totalGrossPrice, 2);

                if (rule.FirstNightPriceSpecified)
                    return Math.Round(100 * rule.FirstNightPrice * rule.Nights / totalGrossPrice, 2);

                return Math.Round(100 * (totalGrossPrice / numberOfNights) * rule.Nights / totalGrossPrice, 2);
            }

            return default;
        }


        DateTimeOffset? GetFirstDeadline(JP_FirstDayCostCancellation? firstDeadline, List<CancellationPolicy> polities)
        {
            DateTimeOffset? date = null;
            if (firstDeadline is not null)
            {
                date = new DateTimeOffset(firstDeadline.Value);

                if (!string.IsNullOrWhiteSpace(firstDeadline.Hour))
                {
                    var hour = TimeOnly.Parse(firstDeadline.Hour); // Indicates the time of the day referred by the FirstDateCostCancellation value. Format: hh: mm
                    date.Value.AddHours(hour.Hour);
                    date.Value.AddMinutes(hour.Minute);
                }
            }

            if (date is null && polities.Count > 0)
                date = polities.Min(p => p.FromDate);

            return date;
        }
    }
}