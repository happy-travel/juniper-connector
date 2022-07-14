using HappyTravel.BaseConnector.Api.Services.Accommodations;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.JuniperConnector.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.JuniperConnector.Api.Services.Accommodations;

public class AccommodationService : IAccommodationService
{
    public AccommodationService(MultilingualAccommodationMapper mapper, JuniperContext context)
    {
        _mapper = mapper;
        _context = context;
    }


    public async Task<List<MultilingualAccommodation>> Get(int skip, int top, DateTimeOffset? modificationDate, CancellationToken cancellationToken)
    {
        var hotels = _context.Hotels.AsQueryable();

        if (modificationDate is not null)
            hotels = hotels.Where(a => a.Modified >= modificationDate);        

        var zones = await _context.Zones.ToListAsync();

        return await hotels
            .OrderBy(h => h.Code)
            .Skip(skip)
            .Take(top)
            .Select(h => _mapper.Map(h, zones))
            .ToListAsync(cancellationToken);
    }


    private readonly MultilingualAccommodationMapper _mapper;
    private readonly JuniperContext _context;
}
