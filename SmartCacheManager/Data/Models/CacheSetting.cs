using SmartCacheManager.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCacheManager.Data
{
    /// <summary>
    /// Represent the settings of cache
    /// </summary>
    public partial class CacheSetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Minimum time difference of search related to now in hours.
        /// حداقل اختلاف ساعت جستجو نسبت به الان
        /// </summary>
        public int MinSearchDiffHours { get; set; }

        /// <summary>
        /// Maximum time difference of search related to now in hours.
        /// حداکثر اختلاف ساعت جستجو نسبت به الان
        /// </summary>
        public int MaxSearchDiffHours { get; set; }

        /// <summary>
        /// Over time difference of search related to now  in hours that disable caching.
        /// لغو کش های بیشتر از زمان
        /// </summary>
        public int OverSearchDiffHours { get; set; }

        /// <summary>
        /// Minimum time of cache in minutes.
        /// حداقل زمان کش به دقیقه
        /// </summary>
        public int MinCacheMinutes { get; set; }

        /// <summary>
        /// Maximum time of cache in minutes.
        /// حداکثر زمان کش به دقیقه
        /// </summary>
        public int MaxCacheMinutes { get; set; }

        /// <summary>
        /// Minimum RPM of recent searchs.
        /// حداقل تعدد جستجو های اخیر
        /// </summary>
        public int RecentSearchMinimumRPM { get; set; }

        /// <summary>
        /// Maximum RPM of recent searchs.
        /// حداکثر تعدد جستجو های اخیر
        /// </summary>
        public int RecentSearchMaxmimumRPM { get; set; }

        /// <summary>
        /// Time duration to calculate RPM in minutes.
        /// بازه اندازه گیری تعدد سرچ های اخیر به دقیقه
        /// </summary>
        public int RpmDurationMinutes { get; set; }

        /// <summary>
        /// Type of supplier
        /// نوع تامین کننده
        /// </summary>
        [MaxLength(50)]
        public string SupplierType { get; set; }

        /// <summary>
        /// Clone a shallow copay of this object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ShallowCopy<T>() where T : CacheSetting
        {
            return (T)MemberwiseClone(); // or DeepCopy with BinarySerializer or JsonSerializer
        }

        public TSupplierType GetSupplierType<TSupplierType>()
        {
            return SupplierType.ConvertTo<TSupplierType>();
        }

        public void SetSupplierType<TSupplierType>(TSupplierType supplierType)
        {
            SupplierType = supplierType.ConvertTo<string>();
        }
    }
}
