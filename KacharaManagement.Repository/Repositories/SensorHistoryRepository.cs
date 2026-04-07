using KacharaManagement.Core.Entities;
using KacharaManagement.Core;
using KacharaManagement.Repository.Data;
using KacharaManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KacharaManagement.Repository.Repositories
{
    public class SensorHistoryRepository : ISensorHistoryRepository
    {
        private readonly GothamDbContext _context;
        public SensorHistoryRepository(GothamDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SensorHistory entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            _context.SensorHistories.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SensorHistory entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.SensorHistories.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<SensorHistory?> GetByIdAsync(int id)
        {
            return await _context.SensorHistories
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<SensorHistory?> GetLatestAsync()
        {
            return await _context.SensorHistories
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<SensorHistory?> GetLatestNeedsTruckAsync()
        {
            return await _context.SensorHistories
                .Where(x => x.NeedsTruck)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SensorHistory>> GetHistoryAsync(int limit)
        {
            return await _context.SensorHistories
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<HistoryPageResponse> GetPagedHistoryAsync(int page = 1, int pageSize = 20, string? source = null, bool? alert = null, bool? needsTruck = null, bool? truckStatusUpdated = null, string? bin1State = null, string? bin2State = null, string? bin3State = null, string? search = null)
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = 20;

            var query = _context.SensorHistories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(source))
            {
                var normalizedSource = source.Trim().ToLower();
                query = query.Where(x => x.Source.ToLower().Contains(normalizedSource));
            }

            if (alert.HasValue)
            {
                query = query.Where(x => (x.Alert == 1) == alert.Value);
            }

            if (needsTruck.HasValue)
            {
                query = query.Where(x => x.NeedsTruck == needsTruck.Value);
            }

            if (truckStatusUpdated.HasValue)
            {
                query = truckStatusUpdated.Value
                    ? query.Where(x =>
                        (x.TruckState != null && x.TruckState != "") ||
                        x.TruckStarted == true ||
                        x.TruckMoving == true ||
                        x.TruckReached == true ||
                        (x.TruckLocation != null && x.TruckLocation != ""))
                    : query.Where(x =>
                        (x.TruckState == null || x.TruckState == "") &&
                        x.TruckStarted != true &&
                        x.TruckMoving != true &&
                        x.TruckReached != true &&
                        (x.TruckLocation == null || x.TruckLocation == ""));
            }

            if (!string.IsNullOrWhiteSpace(bin1State))
            {
                var normalizedBin1 = bin1State.Trim().ToLower();
                query = query.Where(x => x.Bin1State.ToLower() == normalizedBin1);
            }

            if (!string.IsNullOrWhiteSpace(bin2State))
            {
                var normalizedBin2 = bin2State.Trim().ToLower();
                query = query.Where(x => x.Bin2State.ToLower() == normalizedBin2);
            }

            if (!string.IsNullOrWhiteSpace(bin3State))
            {
                var normalizedBin3 = bin3State.Trim().ToLower();
                query = query.Where(x => x.Bin3State.ToLower() == normalizedBin3);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Bin1State.ToLower().Contains(normalizedSearch) ||
                    x.Bin2State.ToLower().Contains(normalizedSearch) ||
                    x.Bin3State.ToLower().Contains(normalizedSearch) ||
                    x.Source.ToLower().Contains(normalizedSearch) ||
                    (x.TruckState != null && x.TruckState.ToLower().Contains(normalizedSearch)) ||
                    (x.TruckLocation != null && x.TruckLocation.ToLower().Contains(normalizedSearch)));
            }

            var totalCount = await query.CountAsync();
            var entities = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(x => new HistoryItem
            {
                Id = x.Id,
                Bin1Fill = x.Bin1Fill,
                Bin1State = x.Bin1State,
                Bin2Light = x.Bin2Light,
                Bin2State = x.Bin2State,
                Bin3Water = x.Bin3Water,
                Bin3State = x.Bin3State,
                Alert = x.Alert,
                NeedsTruck = x.NeedsTruck,
                TruckState = x.TruckState,
                TruckStarted = x.TruckStarted,
                TruckMoving = x.TruckMoving,
                TruckReached = x.TruckReached,
                TruckLatitude = x.TruckLatitude,
                TruckLongitude = x.TruckLongitude,
                TruckLocation = x.TruckLocation,
                Source = x.Source,
                Timestamp = x.CreatedAt.ToString("o")
            }).ToList();

            return new HistoryPageResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
