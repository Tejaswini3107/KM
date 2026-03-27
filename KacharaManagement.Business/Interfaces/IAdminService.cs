using KacharaManagement.Core.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KacharaManagement.Business.Interfaces
{
    public interface IAdminService
    {
        Task<AdminUser?> LoginAsync(string username, string password);
        Task<List<LogEntry>> GetLogsAsync(int limit = 100);
        Task<bool> CreateAdminUserAsync(AdminUser user);
    }
}
