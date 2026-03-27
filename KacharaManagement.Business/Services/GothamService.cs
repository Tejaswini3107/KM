using KacharaManagement.Core.Entities;
using KacharaManagement.Repository.Interfaces;
using KacharaManagement.Business.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KacharaManagement.Business.Services
{
    public class GothamService : IGothamService
    {
        private readonly ISensorHistoryRepository _repo;
        private readonly ILogEntryRepository _logRepo;
        public GothamService(ISensorHistoryRepository repo, ILogEntryRepository logRepo)
        {
            _repo = repo;
            _logRepo = logRepo;
        }

        public async Task AddSensorHistoryAsync(SensorHistory entity)
        {
            try
            {
                await _repo.AddAsync(entity);
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Info",
                    Message = "SensorHistory added",
                    Source = "GothamService.AddSensorHistoryAsync",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "GothamService.AddSensorHistoryAsync",
                    CreatedAt = DateTime.UtcNow
                });
                throw;
            }
        }

        public async Task<SensorHistory?> GetLatestStatusAsync()
        {
            try
            {
                var result = await _repo.GetLatestAsync();
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Info",
                    Message = "Fetched latest SensorHistory",
                    Source = "GothamService.GetLatestStatusAsync",
                    CreatedAt = DateTime.UtcNow
                });
                return result;
            }
            catch (Exception ex)
            {
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "GothamService.GetLatestStatusAsync",
                    CreatedAt = DateTime.UtcNow
                });
                throw;
            }
        }

        public async Task<List<SensorHistory>> GetHistoryAsync(int limit)
        {
            try
            {
                var result = await _repo.GetHistoryAsync(limit);
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Info",
                    Message = $"Fetched {result.Count} SensorHistory records",
                    Source = "GothamService.GetHistoryAsync",
                    CreatedAt = DateTime.UtcNow
                });
                return result;
            }
            catch (Exception ex)
            {
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "GothamService.GetHistoryAsync",
                    CreatedAt = DateTime.UtcNow
                });
                throw;
            }
        }
    }
}
