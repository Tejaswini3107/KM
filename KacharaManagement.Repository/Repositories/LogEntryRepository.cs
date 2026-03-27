using KacharaManagement.Core.Entities;
using KacharaManagement.Repository.Data;
using KacharaManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KacharaManagement.Repository.Repositories
{
    public class LogEntryRepository : ILogEntryRepository
    {
        private readonly GothamDbContext _context;
        public LogEntryRepository(GothamDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LogEntry entry)
        {
            entry.CreatedAt = DateTime.UtcNow;
            _context.LogEntries.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LogEntry>> GetAllAsync(int limit = 100)
        {
            return await _context.LogEntries
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
