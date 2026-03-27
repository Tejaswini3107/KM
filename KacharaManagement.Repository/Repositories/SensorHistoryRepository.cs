using KacharaManagement.Core.Entities;
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

        public async Task<SensorHistory?> GetLatestAsync()
        {
            return await _context.SensorHistories
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
    }
}
