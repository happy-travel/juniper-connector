using CSharpFunctionalExtensions;
using HappyTravel.JuniperConnector.Updater.Infrastructure;
using HappyTravel.JuniperConnector.Updater.Infrastructure.Logging;
using HappyTravel.JuniperConnector.Updater.Service;
using JuniperServiceReference;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace HappyTravel.JuniperConnector.Updater.Workers;

internal class HotelLoader : IUpdateWorker
{
    public HotelLoader(JuniperContentClientService client, ILogger<HotelLoader> logger, HotelUpdater hotelUpdater,
        DateTimeProvider dateTimeProvider)
    {
        _client = client;
        _logger = logger;
        _hotelUpdater = hotelUpdater;
        _dateTimeProvider = dateTimeProvider;
    }


    public async Task Run(CancellationToken cancellationToken)
    {        
        _logger.LogStartingHotelsUpdate();

        var hasErrors = false;
        var modified = _dateTimeProvider.UtcNow();

        await foreach (var (getHotelPortfolioHasError, hotelPortfolio) in GetHotelPortfolio(cancellationToken))
        {
            if (getHotelPortfolioHasError)
                hasErrors = true;
            else
            {
                var (isSuccess, _, hotelContents, _) = await _client.GetHotelContents(hotelPortfolio.Select(x => x.JPCode));

                if (isSuccess)
                    await _hotelUpdater.AddUpdateHotels(hotelContents, modified, cancellationToken);
                else
                    hasErrors = true;
            }                
        }

        if (!hasErrors)
        {
            var affectedRowsCount = await _hotelUpdater.DeactivateNotFetched(modified, cancellationToken);
            _logger.LogDeactivatingCompleted(affectedRowsCount);
        }
        
        _logger.LogFinishHotelsUpdate();             
    }


    private async IAsyncEnumerable<(bool HasError, List<JP_ExtendedHotelInfo>)> GetHotelPortfolio([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string nextPageToken = null;
        do
        {
            var (isSuccess, _, hotelPortfolioResponse, _) = await _client.GetHotelPortfolio(BatchSize, nextPageToken);

            if (isSuccess)
            {
                nextPageToken = hotelPortfolioResponse.NextToken;

                yield return (false, hotelPortfolioResponse.Hotel.ToList());
            }
            else
                yield return (true, default);

        } while (nextPageToken is not null);
    }


    //We can get max 25 hotels from request GetHotelContents
    private const int BatchSize = 25;


    private readonly JuniperContentClientService _client;
    private readonly ILogger<HotelLoader> _logger;   
    private readonly HotelUpdater _hotelUpdater;
    private readonly DateTimeProvider _dateTimeProvider;
}
