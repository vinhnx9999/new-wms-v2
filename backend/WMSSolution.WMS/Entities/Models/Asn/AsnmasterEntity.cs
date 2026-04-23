
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// asn master entity
    /// </summary>
    [Table("asnmaster")]
    public class AsnmasterEntity : BaseModel, ITenantEntity
    {
        #region Property

        /// <summary>
        /// asn_no
        /// </summary>
        public string asn_no { get; set; } = string.Empty;

        /// <summary>
        /// asn_batch
        /// </summary>
        public string asn_batch { get; set; } = string.Empty;

        /// <summary>
        /// estimated_arrival_time
        /// </summary>
        public DateTime estimated_arrival_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// asn_status
        /// </summary>
        public byte asn_status { get; set; } = 0;

        /// <summary>
        /// weight
        /// </summary>  
        public decimal weight { get; set; } = 0;

        /// <summary>
        /// volume
        /// </summary>
        public decimal volume { get; set; } = 0;

        /// <summary>
        /// goods_owner_id
        /// </summary>
        public int goods_owner_id { get; set; } = 0;

        /// <summary>
        /// goods_owner_name
        /// </summary>
        public string goods_owner_name { get; set; } = string.Empty;

        /// <summary>
        /// creator
        /// </summary>
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// create_time
        /// </summary>
        public DateTime create_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// last_update_time
        /// </summary>
        public DateTime last_update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// tenant_id
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;

        /// <summary>
        /// Purchase Order ID
        /// </summary>
        public int? po_id { get; set; }


        /// <summary>
        /// Warehouse Id 
        /// </summary>
        public int warehouse_id { get; set; }

        #endregion

        #region details
        /// <summary>
        /// details
        /// </summary>
        public List<AsnEntity> detailList { get; set; } = new List<AsnEntity>();

        #endregion
    }
}
