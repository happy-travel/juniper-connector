using HappyTravel.JuniperConnector.Data;
using HappyTravel.JuniperConnector.Data.Models;
using HappyTravel.JuniperConnector.Updater.Infrastructure;

namespace HappyTravel.JuniperConnector.Updater.Service;

public class UpdateHistoryService
{
    public UpdateHistoryService(JuniperContext context, DateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }


    public async Task<int> Create()
    {
        var updateLogEntry = new StaticDataUpdateHistoryEntry
        {
            StartTime = _dateTimeProvider.UtcNow(),
            Options = string.Empty
        };

        _context.StaticDataUpdateHistory.Add(updateLogEntry);

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        return updateLogEntry.Id;
    }


    public async Task SetError(int updateId, Exception exception)
    {
        var updateLogEntry = await _context.StaticDataUpdateHistory.FindAsync(updateId);
        updateLogEntry.FinishTime = _dateTimeProvider.UtcNow();
        updateLogEntry.IsSuccess = false;
        updateLogEntry.Message = exception.Message;

        _context.Update(updateLogEntry);

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }


    public async Task SetSuccess(int updateId)
    {
        var updateLogEntry = await _context.StaticDataUpdateHistory.FindAsync(updateId);
        updateLogEntry.FinishTime = _dateTimeProvider.UtcNow();
        updateLogEntry.IsSuccess = true;

        _context.Update(updateLogEntry);

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }


    private readonly JuniperContext _context;
    private readonly DateTimeProvider _dateTimeProvider;
}
