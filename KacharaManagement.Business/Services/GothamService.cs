using KacharaManagement.Core.Entities;
using KacharaManagement.Core;
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

        public async Task UpdateTruckMovementAsync(TruckMovementRequest request)
        {
            try
            {
                var latest = await _repo.GetLatestNeedsTruckAsync() ?? await _repo.GetLatestAsync();
                if (latest == null)
                {
                    return;
                }

                latest.TruckState = NormalizeTruckState(request);
                latest.TruckStarted = request.Started;
                latest.TruckMoving = request.Moving;
                latest.TruckReached = request.Reached;
                latest.TruckLatitude = request.Latitude;
                latest.TruckLongitude = request.Longitude;
                latest.TruckLocation = request.Location;

                await _repo.UpdateAsync(latest);
            }
            catch (Exception ex)
            {
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "GothamService.UpdateTruckMovementAsync",
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

        public async Task<HistoryPageResponse> GetPagedHistoryAsync(int page = 1, int pageSize = 20, string? source = null, bool? alert = null, bool? needsTruck = null, string? bin1State = null, string? bin2State = null, string? bin3State = null, string? search = null)
        {
            try
            {
                return await _repo.GetPagedHistoryAsync(page, pageSize, source, alert, needsTruck, bin1State, bin2State, bin3State, search);
            }
            catch (Exception ex)
            {
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "GothamService.GetPagedHistoryAsync",
                    CreatedAt = DateTime.UtcNow
                });
                throw;
            }
        }

        private static string NormalizeTruckState(TruckMovementRequest request)
        {
            if (request.Reached)
                return "Reached";

            if (request.Moving)
                return "Moving";

            if (request.Started)
                return "Started";

            return string.IsNullOrWhiteSpace(request.State) ? "Idle" : request.State;
        }
    }
}
