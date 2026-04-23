using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Receipt
{
    /// <summary>
    /// Support inbound single pallet 
    /// </summary>
    [Table("inbound_pallet")]
    public class InboundPallet : BaseModel, ITenantEntity
    {
        /// <summary>
        /// Pallet Code
        /// </summary>
        [Column("pallet_code")]
        public string PalletCode { get; set; } = default!;

        /// <summary>
        /// Pallet RFID
        /// </summary>
        [Column("pallet_rfid")]
        public string? PalletRFID { get; set; }

        /// <summary>
        /// location id of pallet
        /// </summary>
        [Column("location_id")]
        public int LocationId { get; set; }

        /// <summary>
        /// Description of the pallet
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates if the pallet contains mixed SKUs
        /// default is not mixing skus in pallet
        /// </summary>
        [Column("is_mix")]
        public bool IsMixed { get; set; } = false;

        /// <summary>
        /// Create time
        /// </summary>
        [Column("created_time")]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update time
        /// </summary>  
        [Column("last_updated_time")]
        public DateTime LastUpdatedTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// tenant id 
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;

        /// <summary>
        /// Details
        /// </summary>
        public virtual ICollection<InboundPalletDetail> Details { get; set; } = [];
    }
}
