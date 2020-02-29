using SmartCacheManager.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCacheManager.Services
{
    /// <summary>
    /// Service for LimitSetting
    /// </summary>
    /// <typeparam name="TLimitSetting">Type of LimitSetting</typeparam>
    public interface ILimitSettingService<TLimitSetting>
        where TLimitSetting : LimitSetting, new()
    {
        /// <summary>
        /// Get LimitSetting from cache by specified SupplierType or if not exists create a global LimitSetting
        /// </summary>
        /// <param name="supplierType">Type of supplier</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>List of TLimitSetting</returns>
        Task<List<TLimitSetting>> GetFromCacheBySupplierTypeAsync<TSupplierType>(TSupplierType supplierType, CancellationToken cancellationToken = default);
    }
}
