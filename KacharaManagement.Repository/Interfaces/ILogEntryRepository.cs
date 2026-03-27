using KacharaManagement.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KacharaManagement.Repository.Interfaces
{
    public interface ILogEntryRepository
    {
        Task AddAsync(LogEntry entry);
        Task<List<LogEntry>> GetAllAsync(int limit = 100);
    }
}
