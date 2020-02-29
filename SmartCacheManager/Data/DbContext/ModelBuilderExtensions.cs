using SmartCacheManager.Utilities;
using Microsoft.EntityFrameworkCore;

namespace SmartCacheManager.Data
{
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Add caching models to model builder
        /// </summary>
        /// <param name="modelBuilder">modelBuilder</param>
        public static void AddCacheModels(this ModelBuilder modelBuilder)
        {
            modelBuilder.NotNull(nameof(modelBuilder));

            //Register base types
            modelBuilder.Entity<CacheSetting>();
            modelBuilder.Entity<LimitSetting>();
            modelBuilder.Entity<IncomingRequest>(typeBuilder =>
            {
                //typeBuilder.HasIndex(p => p.CreatedAt);
                //typeBuilder.HasIndex(p => p.HashCode);
                typeBuilder.HasIndex(p => new { p.HashCode, p.CreatedAt });
            });
            modelBuilder.Entity<OutgoingRequest>(typeBuilder =>
            {
                typeBuilder.HasIndex(p => new { p.HashCode, p.CreatedAt });
            });
        }
    }
}
