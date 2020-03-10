using SmartCacheManager.Data;
using System.Threading.Tasks;
using System.Threading;

namespace SmartCacheManager.Services
{
    public interface ISearchHistoryService<TCacheSetting, TLimitSetting>
        where TCacheSetting : CacheSetting, new()
        where TLimitSetting : LimitSetting, new()
    {
        /// <summary>
        /// Add an incoming search request
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <param name="incomingPrefix">Icoming pattern prefix</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task AddIncommingAsync<TSupplierType>(string incomingPrefix, TSupplierType supplierType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add an outgoing search request
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <param name="outgoingPrefix">Outgoing pattern prefix</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task AddOutgoingAsync<TSupplierType>(string outgoingPrefix, TSupplierType supplierType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get current RPM based on specified prefix and supplier type
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <param name="incomingPrefix">Icoming pattern prefix</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Current RPM</returns>
        Task<decimal> GetRpmAsync<TSupplierType>(string incomingPrefix, TSupplierType supplierType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Specifies whether or not the limitation is reached
        /// </summary>
        /// <typeparam name="TSupplierType">Type of SupplierType</typeparam>
        /// <param name="outgoingPrefix">Outgoing pattern prefix</param>
        /// <param name="supplierType">Supplier type</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Return true if limitation is reached</returns>
        Task<bool> IsLimitationReachedAsync<TSupplierType>(string outgoingPrefix, TSupplierType supplierType, CancellationToken cancellationToken = default);
    }
}
