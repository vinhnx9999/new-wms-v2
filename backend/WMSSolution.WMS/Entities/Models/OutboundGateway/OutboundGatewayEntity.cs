using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.OutboundGateway
{
    /// <summary>
    /// Outbound Gateway entity
    /// </summary>
    [Table("outbound_gateway")]
    public class OutboundGatewayEntity : BaseModel, ITenantEntity
    {
        /// <summary>
        /// gateway name
        /// </summary>

        [Column("gateway_name")]
        public required string GatewayName { get; set; }
        /// <summary>
        /// warehouse id 
        /// </summary>
        [Column("warehouse_id")]
        public required int WarehouseId { get; set; }
        /// <summary>
        /// create time
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// last update time
        /// </summary>
        [Column("last_update_time")]
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// reference to Outbound if have
        /// </summary>
        [Column("ref_receipt")]
        public int? RefReceipt { get; set; }

        /// <summary>
        /// tenant id
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;
    }
}
