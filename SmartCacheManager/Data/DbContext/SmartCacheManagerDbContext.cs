using Microsoft.EntityFrameworkCore;

namespace SmartCacheManager.Data
{
    /// <summary>
    /// DbContext for caching models
    /// </summary>
    public class SmartCacheManagerDbContext : DbContext
    {
        public SmartCacheManagerDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddCacheModels();

            base.OnModelCreating(modelBuilder);
        }
    }
}
