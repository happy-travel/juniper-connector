using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Data;
using JuniperServiceReference;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.JuniperConnector.Updater.Service;

public class HotelUpdater
{
    public HotelUpdater(JuniperContext context, JuniperSerializer serializer)
    {
        _context = context;
        _serializer = serializer;
    }


    public async Task AddUpdateHotels(List<JP_HotelContent> hotels, DateTimeOffset modified, CancellationToken cancellationToken)
    {
        foreach (var hotel in hotels)
        {
            var updateHotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Code == hotel.Code, cancellationToken);            

            if (updateHotel is not null)
            {
                updateHotel.Data = _serializer.Serialize(hotel);
                updateHotel.Modified = modified;
                updateHotel.IsActive = true;

                _context.Update(updateHotel);
            }
            else
            {
                await _context.Hotels.AddAsync(new Data.Models.Hotel
                {
                    Code = hotel.Code,
                    Data = _serializer.Serialize(hotel),
                    Modified = modified,
                    IsActive = true
                }, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _context.ChangeTracker.Clear();
    }


    public async Task<int> DeactivateNotFetched(DateTimeOffset modified, CancellationToken cancellationToken)
       => await _context.Database.ExecuteSqlInterpolatedAsync(
            @$"UPDATE ""Hotels"" SET ""IsActive"" = false WHERE ""IsActive"" = true AND ""Modified"" < {modified}", 
            cancellationToken);



    private readonly JuniperContext _context;
    private readonly JuniperSerializer _serializer;
}
