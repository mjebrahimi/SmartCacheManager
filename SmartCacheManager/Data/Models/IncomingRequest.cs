using SmartCacheManager.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCacheManager.Data
{
    #region TODO
    //========= R&D to choose the following scenarios =========
    //Clasterd or non-clasterd indexing ?
    //Seprate or not incoming/outgoing tables ?
    //Indexing on bool(bit) column for IsOutgoing ?
    //AlternateKey or HasIndex multiple column or IncludeProperties ? https://docs.microsoft.com/en-us/ef/core/modeling/indexes
    //Add only or Update previus record ?
    //Json data type for sqlserver ?
    //PK on Id or IncomingHash ?
    //Datetime or Ticks for CreatedAt ?
    //Batching period sql service 
    //https://github.com/serilog/serilog-sinks-periodicbatching/blob/dev/src/Serilog.Sinks.PeriodicBatching/Sinks/PeriodicBatching/PeriodicBatchingSink.cs
    //https://github.com/serilog/serilog-sinks-mssqlserver/blob/2d1fa1bcbf36788d218dcfede758b046ed5b9f4c/src/Serilog.Sinks.MSSqlServer/Sinks/MSSqlServer/MSSqlServerSink.cs
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
        /// Hash for indexing incomming requests based on search model for RPM
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
