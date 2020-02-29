using SmartCacheManager.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCacheManager.Data
{
    /// <summary>
    /// Represent the settings of limitation
    /// </summary>
    public partial class LimitSetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Count of request for limitation
        /// تعداد محدودیت درخواست ها
        /// </summary>
        public int RequestLimit { get; set; }

        /// <summary>
        /// Type of supplier
        /// نوع تامین کننده
        /// </summary>
        [MaxLength(50)]
        public string SupplierType { get; set; }

        /// <summary>
        /// Enable limitation mode
        /// فعال کردن قابلیت محدود سازی
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Duration of limitation in hours
        /// مدت زمان محدودیت بر حسب ساعت
        /// </summary>
        public int LimitDurationHours { get; set; }

        /// <summary>
        /// Extended time in hours
        /// مدت زمان تمدید محدودیت پس از هر خرید
        /// </summary>
        public int ExtendedTimeHours { get; set; }

        /// <summary>
        /// Get limit duration in TimeSpan
        /// </summary>
        [NotMapped]
        public TimeSpan LimitDuration => TimeSpan.FromHours(LimitDurationHours);

        /// <summary>
        /// Get extended time in TimeSpan
        /// </summary>
        [NotMapped]
        public TimeSpan Extended => TimeSpan.FromHours(ExtendedTimeHours);

        /// <summary>
        /// Clone a shallow copay of this object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ShallowCopy<T>() where T : LimitSetting
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
