using KacharaManagement.Core.Entities;
using KacharaManagement.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KacharaManagement.Repository.Interfaces
{
    public interface ISensorHistoryRepository
    {
        Task AddAsync(SensorHistory entity);
        Task UpdateAsync(SensorHistory entity);
        Task<SensorHistory?> GetLatestAsync();
        Task<SensorHistory?> GetLatestNeedsTruckAsync();
        Task<List<SensorHistory>> GetHistoryAsync(int limit);
        Task<HistoryPageResponse> GetPagedHistoryAsync(int page = 1, int pageSize = 20, string? source = null, bool? alert = null, bool? needsTruck = null, string? bin1State = null, string? bin2State = null, string? bin3State = null, string? search = null);
    }
}
