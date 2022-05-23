using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Data;
using HappyTravel.JuniperConnector.Updater.Infrastructure;
using JuniperServiceReference;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.JuniperConnector.Updater.Service;

public class HotelsUpdater
{
    public HotelsUpdater(JuniperContext context, JuniperSerializer serializer, DateTimeProvider dateTimeProvider)
    {
        _context = context;
        _serializer = serializer;
        _dateTimeProvider = dateTimeProvider;
    }


    public async Task AddUpdateHotels(List<JP_HotelContent> hotels, CancellationToken cancellationToken)
    {
        foreach (var hotel in hotels)
        {
            var updateHotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Code == hotel.Code, cancellationToken);            

            if (updateHotel is not null)
            {
                updateHotel.Data = _serializer.Serialize(hotel);
                updateHotel.Modified = _dateTimeProvider.UtcNow();
                updateHotel.IsActive = true;

                _context.Update(updateHotel);
            }
            else
            {
                await _context.Hotels.AddAsync(new Data.Models.Hotel
                {
                    Code = hotel.Code,
                    Data = _serializer.Serialize(hotel),
                    Modified = _dateTimeProvider.UtcNow(),
                    IsActive = true
                }, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _context.ChangeTracker.Clear();
    }


    public async Task DeactivateAllHotels(CancellationToken cancellationToken)
    {
        var entityType = _context.Model.FindEntityType(typeof(Data.Models.Hotel));
        var tableName = entityType.GetTableName();

        await _context.Database.ExecuteSqlRawAsync($"UPDATE \"{tableName}\" SET \"IsActive\" = false",
            cancellationToken: cancellationToken);
    }


    private readonly JuniperContext _context;
    private readonly JuniperSerializer _serializer;
    private readonly DateTimeProvider _dateTimeProvider;
}
