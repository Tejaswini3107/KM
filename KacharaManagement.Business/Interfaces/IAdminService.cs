using KacharaManagement.Core.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using KacharaManagement.Core;

namespace KacharaManagement.Business.Interfaces
{
    public interface IAdminService
    {
        Task<AdminUser?> LoginAsync(string username, string password);
        Task<LogPageResponse> GetLogsAsync(int page = 1, int pageSize = 20, string? level = null, string? source = null, string? search = null);
        Task<bool> CreateAdminUserAsync(AdminUser user);
    }
}
