using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCacheManager.Data
{
    /// <summary>
    /// Interface of service for store (CRUD) of an Entity
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity</typeparam>
    public interface IGenericStore<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// Query all entities AsTracking
        /// </summary>
        IQueryable<TEntity> Table { get; }

        /// <summary>
        /// Query all entities AsNoTracking
        /// </summary>
        IQueryable<TEntity> TableNoTracking { get; }

        /// <summary>
        /// Insert enitites
        /// </summary>
        /// <param name="entities">Entities to insert</param>
        void Add(IEnumerable<TEntity> entities);

        /// <summary>
        /// Insert an entity
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        void Add(TEntity entity);

        /// <summary>
        /// Insert enitites async
        /// </summary>
        /// <param name="entities">Entities to insert</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert an entity async
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="entities">Entities to delete</param>
        void Delete(IEnumerable<TEntity> entities);

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Delete an entity by Id (without additional select)
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <param name="id">Id of entity</param>
        void Delete<TKey>(TKey id);

        /// <summary>
        /// Delete entities async
        /// </summary>
        /// <param name="entities">Entities to delete</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete an entity async
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete an entity by Id async (without additional select)
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <param name="id">Id of entity</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task DeleteAsync<TKey>(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all entities from cache or fetch from database
        /// </summary>
        /// <param name="expirationMinutes">Cache expiration in minutes</param>
        /// <returns>List of entities</returns>
        List<TEntity> GetAllFromCache(int? expirationMinutes = null);

        /// <summary>
        /// Get all entities from cache or fetch from database async
        /// </summary>
        /// <param name="expirationMinutes">Cache expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>List of entities</returns>
        Task<List<TEntity>> GetAllFromCacheAsync(int? expirationMinutes = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get and entity by ids
        /// </summary>
        /// <param name="ids">The values of the primary key for the entity to be found</param>
        /// <returns>The entity found, or null</returns>
        TEntity GetById(params object[] ids);

        /// <summary>
        /// Get and entity by ids async
        /// </summary>
        /// <param name="ids">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>The entity found, or null</returns>
        ValueTask<TEntity> GetByIdAsync(IEnumerable<object> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get and entity by Id from cache or fetch from database
        /// </summary>
        /// <param name="ids">The values of the primary key for the entity to be found.</param>
        /// <param name="expirationMinutes">Cache expiration in minutes</param>
        /// <returns>The entity found, or null from cache</returns>
        TEntity GetByIdFromCache(IEnumerable<object> ids, int? expirationMinutes = null);

        /// <summary>
        /// Get and entity by Id from cache or fetch from database async
        /// </summary>
        /// <param name="ids">The values of the primary key for the entity to be found.</param>
        /// <param name="expirationMinutes">Cache expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>The entity found, or null from cache</returns>
        Task<TEntity> GetByIdFromCacheAsync(IEnumerable<object> ids, int? expirationMinutes = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update entities
        /// </summary>
        /// <param name="entities">Entities to update</param>
        void Update(IEnumerable<TEntity> entities);

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        void Update(TEntity entity);

        /// <summary>
        /// Update entities async
        /// </summary>
        /// <param name="entities">Entities to update</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an entity async
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}