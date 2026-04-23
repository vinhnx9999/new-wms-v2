using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums.Inbound;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// asnsort  entity
    /// 
    /// </summary>
    [Table("asnsort")]
    public class AsnsortEntity : BaseModel, ITenantEntity
    {
        #region Property

        /// <summary>
        /// asn_id
        /// </summary>
        public int asn_id { get; set; } = 0;

        /// <summary>
        /// sorted_qty
        /// </summary>
        public int sorted_qty { get; set; } = 0;

        /// <summary>
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;

        /// <summary>
        /// putaway qty
        /// </summary>
        public int putaway_qty { get; set; } = 0;

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
        /// is_valid
        /// </summary>
        public bool is_valid { get; set; } = true;

        /// <summary>
        /// status
        /// </summary>
        [Column("status")]
        public AsnSortStatusEnum status { get; set; } = AsnSortStatusEnum.Pending;

        /// <summary>
        /// target location id
        /// </summary>
        public int good_location_id { get; set; } = 0;

        /// <summary>
        /// pallet id 
        /// </summary>
        public int pallet_id { get; set; } = 0;

        /// <summary>
        /// tenant_id
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;

        #endregion
    }
}
