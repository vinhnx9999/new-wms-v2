using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// stock entity
    /// </summary>
    [Table("stock")]
    public class StockEntity : BaseModel, ITenantEntity
    {
        #region Property

        /// <summary>
        /// sku_id
        /// </summary>
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// goods_location_id
        /// </summary>
        public int goods_location_id { get; set; } = 0;

        /// <summary>
        /// qty
        /// </summary>
        public int qty { get; set; } = 0;

        /// <summary>
        /// actual qty 
        /// </summary>
        public decimal actual_qty { get; set; } = 0;

        /// <summary>
        /// goods_owner_id
        /// </summary>
        public int goods_owner_id { get; set; } = 0;

        /// <summary>
        /// is_freeze
        /// </summary>
        public bool is_freeze { get; set; } = false;

        /// <summary>
        /// last_update_time
        /// </summary>
        [ConcurrencyCheck]
        public DateTime last_update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// tenant_id
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 0;

        /// <summary>
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;

        /// <summary>
        /// expiry_date
        /// </summary>
        public DateTime? expiry_date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// price
        /// </summary>
        public decimal price { get; set; } = 0;

        /// <summary>
        /// putaway_date
        /// </summary>
        [Column("putaway_date")]
        public DateTime PutAwayDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Supplier ID
        /// </summary>
        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        /// <summary>
        /// ASN Master ID
        /// </summary>
        [Column("asn_master_id")]
        public int? AsnMasterID { get; set; }

        /// <summary>
        /// Pallet Code 
        /// </summary>
        [Column("pallet_code")]
        public string? Palletcode { get; set; }

        #endregion Property
    }
}