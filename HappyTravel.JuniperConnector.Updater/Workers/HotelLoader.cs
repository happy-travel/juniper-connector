using HappyTravel.JuniperConnector.Common.JuniperService;
using HappyTravel.JuniperConnector.Updater.Infrastructure.Logging;
using HappyTravel.JuniperConnector.Updater.Service;
using JuniperServiceReference;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace HappyTravel.JuniperConnector.Updater.Workers;

internal class HotelLoader : IUpdateWorker
{
    public HotelLoader(IJuniperServiceClient client, ILogger<HotelLoader> logger, UpdateHistoryService updateHistoryService, HotelsUpdater hotelsUpdater)
    {
        _client = client;
        _logger = logger;        
        _updateHistoryService = updateHistoryService;
        _hotelsUpdater = hotelsUpdater;
    }


    public async Task Run(CancellationToken cancellationToken)
    {
        var updateId = await _updateHistoryService.Create();
        _logger.LogStartingHotelsUpdate(updateId);

        try
        {
            _logger.LogDeactivateAllHotels();
            await _hotelsUpdater.DeactivateAllHotels(cancellationToken);            

            await foreach (var hotelPortfolio in GetHotelPortfolio(cancellationToken))
            {
                var hotelContents = await _client.GetHotelContents(hotelPortfolio.Select(x => x.JPCode));

                await _hotelsUpdater.AddUpdateHotels(hotelContents, cancellationToken);
            }            

            await _updateHistoryService.SetSuccess(updateId);
            _logger.LogFinishHotelsUpdate(updateId);
        }
        catch (Exception ex)
        {
            _logger.LogHotelLoaderException(ex);
            await _updateHistoryService.SetError(updateId, ex);
        }        
    }


    private async IAsyncEnumerable<List<JP_ExtendedHotelInfo>> GetHotelPortfolio([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string nextPageToken = null;
        do
        {
            var hotelPortfolioResponse = await _client.GetHotelPortfolio(BatchSize, nextPageToken);

            nextPageToken = hotelPortfolioResponse.NextToken;

            yield return hotelPortfolioResponse.Hotel.ToList();

        } while (nextPageToken is not null);
    }


    //We can get max 25 hotels from request GetHotelContents
    private const int BatchSize = 25;


    private readonly IJuniperServiceClient _client;
    private readonly ILogger<HotelLoader> _logger;    
    private readonly UpdateHistoryService _updateHistoryService;
    private readonly HotelsUpdater _hotelsUpdater;
}
