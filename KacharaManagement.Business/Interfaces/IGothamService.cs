using KacharaManagement.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KacharaManagement.Business.Interfaces
{
    public interface IGothamService
    {
        Task AddSensorHistoryAsync(SensorHistory entity);
        Task<SensorHistory?> GetLatestStatusAsync();
        Task<List<SensorHistory>> GetHistoryAsync(int limit);
        // Logging methods to be added
    }
}
