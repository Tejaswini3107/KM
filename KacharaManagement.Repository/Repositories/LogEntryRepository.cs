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

        public async Task<LogPageResponse> GetPagedAsync(int page = 1, int pageSize = 20, string? level = null, string? source = null, string? search = null)
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = 20;

            var query = _context.LogEntries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(level))
            {
                var normalizedLevel = level.Trim().ToLower();
                query = query.Where(x => x.Level.ToLower() == normalizedLevel);
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                var normalizedSource = source.Trim().ToLower();
                query = query.Where(x => x.Source.ToLower().Contains(normalizedSource));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Message.ToLower().Contains(normalizedSearch) ||
                    (x.RequestBody != null && x.RequestBody.ToLower().Contains(normalizedSearch)) ||
                    (x.ResponseBody != null && x.ResponseBody.ToLower().Contains(normalizedSearch)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new LogPageResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
