using SmartCacheManager.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCacheManager.Services
{
    /// <summary>
    /// Service for CacheSetting
    /// </summary>
    /// <typeparam name="TCacheSetting">Type of CacheSetting</typeparam>
    public interface ICacheSettingService<TCacheSetting>
        where TCacheSetting : CacheSetting, new()
    {
        /// <summary>
        /// Get CacheSetting from cache by specified SupplierType or if not exists create a global CacheSetting
        /// </summary>
        /// <param name="supplierType">Type of supplier</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>TCacheSetting</returns>
        Task<TCacheSetting> GetFromCacheBySupplierTypeAsync<TSupplierType>(TSupplierType supplierType, CancellationToken cancellationToken = default);
    }
}
