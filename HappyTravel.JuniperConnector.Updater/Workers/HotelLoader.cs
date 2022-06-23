using CSharpFunctionalExtensions;
using HappyTravel.JuniperConnector.Updater.Infrastructure.Logging;
using HappyTravel.JuniperConnector.Updater.Service;
using JuniperServiceReference;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace HappyTravel.JuniperConnector.Updater.Workers;

internal class HotelLoader : IUpdateWorker
{
    public HotelLoader(JuniperContentClientService client, ILogger<HotelLoader> logger, HotelUpdater hotelUpdater)
    {
        _client = client;
        _logger = logger;
        _hotelUpdater = hotelUpdater;
    }


    public async Task Run(CancellationToken cancellationToken)
    {        
        _logger.LogStartingHotelsUpdate();
               
        _logger.LogDeactivateAllHotels();
        await _hotelUpdater.DeactivateAllHotels(cancellationToken);            

        await foreach (var hotelPortfolio in GetHotelPortfolio(cancellationToken))
        {
            var (isSuccess, _, hotelContents, _) = await _client.GetHotelContents(hotelPortfolio.Select(x => x.JPCode));

            if (isSuccess)
                await _hotelUpdater.AddUpdateHotels(hotelContents, cancellationToken);
        }            

        _logger.LogFinishHotelsUpdate();             
    }


    private async IAsyncEnumerable<List<JP_ExtendedHotelInfo>> GetHotelPortfolio([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string nextPageToken = null;
        do
        {
            var (isSuccess, _, hotelPortfolioResponse, _) = await _client.GetHotelPortfolio(BatchSize, nextPageToken);

            if (isSuccess)
            {
                nextPageToken = hotelPortfolioResponse.NextToken;

                yield return hotelPortfolioResponse.Hotel.ToList();
            }
            else
                yield break;

        } while (nextPageToken is not null);
    }


    //We can get max 25 hotels from request GetHotelContents
    private const int BatchSize = 25;


    private readonly JuniperContentClientService _client;
    private readonly ILogger<HotelLoader> _logger;   
    private readonly HotelUpdater _hotelUpdater;
}
