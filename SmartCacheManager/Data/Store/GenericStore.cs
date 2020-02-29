using SmartCacheManager.Caching;
using SmartCacheManager.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCacheManager.Data
{
    /// <summary>
    /// Service for store (CRUD) of an Entity
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity</typeparam>
    public class GenericStore<TEntity> : IGenericStore<TEntity>
        where TEntity : class, new()
    {
        protected readonly DbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;
        protected readonly ICacheManager CacheManager;

        public static readonly string CacheKeyById = $"SmartCacheManager_GenericStore_{typeof(TEntity).FullName}_ById-{{0}}";
        public static readonly string CacheKeyAll = $"SmartCacheManager_GenericStore_{typeof(TEntity).FullName}_All";
        public static readonly string CacheKeyPrefix = $"SmartCacheManager_GenericStore_{typeof(TEntity).FullName}";

        /// <summary>
        /// Query all entities AsTracking
        /// </summary>
        public virtual IQueryable<TEntity> Table => DbSet;

        /// <summary>
        /// Query all entities AsNoTracking
        /// </summary>
        public virtual IQueryable<TEntity> TableNoTracking => DbSet.AsNoTracking();

        public GenericStore(DbContext dbContext, ICacheManager cacheManager)
        {
            DbContext = dbContext.NotNull(nameof(dbContext));
            CacheManager = cacheManager.NotNull(nameof(cacheManager));

            DbSet = DbContext.Set<TEntity>();
        }

        #region Sync Methods
        /// <summary>
        /// Get and entity by ids
        /// </summary>
        /// <param name="ids">The values of the primary key for the entity to be found</param>
        /// <returns>The entity found, or null</returns>
        public virtual TEntity GetById(params object[] ids)
        {
            return DbSet.Find(ids);
        }

        /// <summary>
        /// Insert an entity
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        public virtual void Add(TEntity entity)
        {
            DbSet.Add(entity);
            SaveChanges();
        }

        /// <summary>
        /// Insert enitites
        /// </summary>
        /// <param name="entities">Entities to insert</param>
        public virtual void Add(IEnumerable<TEntity> entities)
        {
            DbSet.AddRange(entities);
            SaveChanges();
        }

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
            SaveChanges();
        }

        /// <summary>
        /// Update entities
        /// </summary>
        /// <param name="entities">Entities to update</param>
        public virtual void Update(IEnumerable<TEntity> entities)
        {
            DbSet.UpdateRange(entities);
            SaveChanges();
        }

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        public virtual void Delete(TEntity entity)
        {
            DbSet.Remove(entity);
            SaveChanges();
        }

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="entities">Entities to delete</param>
        public virtual void Delete(IEnumerable<TEntity> entities)
        {
            DbSet.RemoveRange(entities);
            SaveChanges();
        }

        /// <summary>
        /// Delete an entity by Id (without additional select)
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <param name="id">Id of entity</param>
        public virtual void Delete<TKey>(TKey id)
        {
            dynamic entity = new TEntity();
            entity.Id = id;
            DbContext.Entry((TEntity)entity).State = EntityState.Deleted;
            SaveChanges();
        }

        /// <summary>
        /// Get and entity by Id from cache or fetch from database
        /// </summary>
        /// <param name="ids">The values of the primary key for the entity to be found.</param>
        /// <param name="expirationMinutes">Cache expiration in minutes</param>
        /// <returns>The entity found, or null from cache</returns>
        public virtual TEntity GetByIdFromCache(IEnumerable<object> ids, int? expirationMinutes = null)
        {
            var key = string.Format(CacheKeyById, string.Join("-", ids));
            return CacheManager.Get(key, () =>
            {
                return GetById(ids);
            }, expirationMinutes);
        }

        /// <summary>
        /// Get all entities from cache or fetch from database
        /// </summary>
        /// <param name="expirationMinutes">Cache expiration in minutes</param>
        /// <returns>List of entities</returns>
        public virtual List<TEntity> GetAllFromCache(int? expirationMinutes = null)
        {
            return CacheManager.Get(CacheKeyAll, () =>
            {
                return TableNoTracking.ToList();
            }, expirationMinutes);
        }

        /// <summary>
        /// Saves all changes made in this context to the database and remove cache by prefix
        /// </summary>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        protected virtual void SaveChanges()
        {
            DbContext.SaveChanges();
            CacheManager.RemoveByPrefix(CacheKeyPrefix);
        }
        #endregion

        #region Async Methods
        /// <summary>
        /// Get and entity by ids async
        /// </summary>
        /// <param name="ids">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>The entity found, or null</returns>
        public virtual Task<TEntity> GetByIdAsync(IEnumerable<object> ids, CancellationToken cancellationToken = default)
        {
            return DbSet.FindAsync(ids, cancellationToken);
        }

        /// <summary>
        /// Insert an entity async
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity);
            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert enitites async
        /// </summary>
        /// <param name="entities">Entities to insert</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public virtual async Task AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await DbSet.AddRangeAsync(entities);
            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Update an entity async
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbSet.Update(entity);
            return SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Update entities async
        /// </summary>
        /// <param name="entities">Entities to update</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public virtual Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            DbSet.UpdateRange(entities);
            return SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Delete an entity async
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbSet.Remove(entity);
            return SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Delete entities async
        /// </summary>
        /// <param name="entities">Entities to delete</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public virtual Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            DbSet.RemoveRange(entities);
            return SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Delete an entity by Id async (without additional select)
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <param name="id">Id of entity</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public virtual Task DeleteAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
        {
            dynamic entity = new TEntity();
            entity.Id = id;
            DbContext.Entry((TEntity)entity).State = EntityState.Deleted;
            return SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Get and entity by Id from cache or fetch from database async
        /// </summary>
        /// <param name="ids">The values of the primary key for the entity to be found.</param>
        /// <param name="expirationMinutes">Cache expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>The entity found, or null from cache</returns>
        public virtual Task<TEntity> GetByIdFromCacheAsync(IEnumerable<object> ids, int? expirationMinutes = null, CancellationToken cancellationToken = default)
        {
            var key = string.Format(CacheKeyById, string.Join("-", ids));
            return CacheManager.GetAsync(key, () =>
            {
                return GetByIdAsync(ids, cancellationToken);
            }, expirationMinutes);
        }

        /// <summary>
        /// Get all entities from cache or fetch from database async
        /// </summary>
        /// <param name="expirationMinutes">Cache expiration in minutes</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>List of entities</returns>
        public virtual Task<List<TEntity>> GetAllFromCacheAsync(int? expirationMinutes = null, CancellationToken cancellationToken = default)
        {
            return CacheManager.GetAsync(CacheKeyAll, () =>
            {
                return TableNoTracking.ToListAsync(cancellationToken);
            }, expirationMinutes);
        }

        /// <summary>
        /// Saves all changes made in this context to the database and remove cache by prefix
        /// </summary>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        protected virtual async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await CacheManager.RemoveByPrefixAsync(CacheKeyPrefix).ConfigureAwait(false);
        }
        #endregion
    }
}
