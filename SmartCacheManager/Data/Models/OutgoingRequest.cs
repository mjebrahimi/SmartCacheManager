using System;
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
        /// Hash for indexing outgoing requests based on supplier type for limitation
        /// </summary>
        public long HashCode { get; set; }
    }
}
