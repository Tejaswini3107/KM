using KacharaManagement.Core;
using KacharaManagement.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KacharaManagement.Business.Interfaces
{
    public interface IGothamService
    {
        Task AddSensorHistoryAsync(SensorHistory entity);
        Task UpdateTruckMovementAsync(TruckMovementRequest request);
        Task<SensorHistory?> GetLatestStatusAsync();
        Task<List<SensorHistory>> GetHistoryAsync(int limit);
        Task<HistoryPageResponse> GetPagedHistoryAsync(int page = 1, int pageSize = 20, string? source = null, bool? alert = null, bool? needsTruck = null, bool? truckStatusUpdated = null, string? bin1State = null, string? bin2State = null, string? bin3State = null, string? search = null);
        // Logging methods to be added
    }
}
