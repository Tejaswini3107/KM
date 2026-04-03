using KacharaManagement.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using KacharaManagement.Core;

namespace KacharaManagement.Repository.Interfaces
{
    public interface ILogEntryRepository
    {
        Task AddAsync(LogEntry entry);
        Task<List<LogEntry>> GetAllAsync(int limit = 100);
        Task<LogPageResponse> GetPagedAsync(int page = 1, int pageSize = 20, string? level = null, string? source = null, string? search = null);
    }
}
