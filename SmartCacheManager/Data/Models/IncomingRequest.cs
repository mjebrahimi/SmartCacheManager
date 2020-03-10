using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCacheManager.Data
{
    #region TODO
    //========= R&D to choose the following scenarios =========
    //Clasterd or non-clasterd indexing ?
    //AlternateKey or HasIndex multiple column or IncludeProperties ? https://docs.microsoft.com/en-us/ef/core/modeling/indexes
    //Datetime or Ticks for CreatedAt ?
    #endregion

    /// <summary>
    /// Represent an incomming request of search history
    /// </summary>
    public partial class IncomingRequest
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// DateTime of creation
        /// </summary>
        public DateTime CreatedAt { get; set; } //CreatedAt

        /// <summary>
        /// Hash for indexing incomming requests based on search model for RPM
        /// </summary>
        public long HashCode { get; set; }
    }
}
