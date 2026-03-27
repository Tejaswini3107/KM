using KacharaManagement.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KacharaManagement.Repository.Interfaces
{
    public interface ISensorHistoryRepository
    {
        Task AddAsync(SensorHistory entity);
        Task<SensorHistory?> GetLatestAsync();
        Task<List<SensorHistory>> GetHistoryAsync(int limit);
    }
}
