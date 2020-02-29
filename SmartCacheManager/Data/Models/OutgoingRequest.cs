using SmartCacheManager.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCacheManager.Data
{
    /// <summary>
    /// Represent an outgoing request of search history
    /// </summary>
    public partial class OutgoingRequest
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// DateTime of creation
        /// </summary>
        public DateTime CreatedAt { get; set; } //CreatedAt

        /// <summary>
        /// Type of supplier
        /// نوع تامین کننده
        /// </summary>
        [MaxLength(50)]
        public string SupplierType { get; set; }

        /// <summary>
        /// Prefix of search history key
        /// </summary>
        [MaxLength(200)]
        public string KeyPrefix { get; set; }

        /// <summary>
        /// Hash for indexing outgoing requests based on supplier type for limitation
        /// </summary>
        public long HashCode { get; set; }

        /// <summary>
        /// Json serialized of search model
        /// </summary>
        public string SearchModel { get; set; }

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
