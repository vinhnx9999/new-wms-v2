using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Sku
{
    /// <summary>
    /// specification  entity 
    /// As known as Mã quy cách 
    /// </summary>
    [Table("specification")]
    public class SpecificationEntity : BaseModel, ITenantEntity
    {
        /// <summary>
        /// specification_code
        /// </summary>
        [Column("specification_code")]
        public string specification_code { get; set; } = string.Empty;

        /// <summary>
        /// specification_name
        /// </summary>
        [Column("specification_name")]
        public string specification_name { get; set; } = string.Empty;

        /// <summary>
        /// is_duplicate
        /// </summary>
        [Column("is_duplicate")]
        public bool is_duplicate { get; set; } = false;

        /// <summary>
        /// create_time
        /// </summary>
        [Column("create_time")]
        public DateTime create_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// update_times
        /// </summary>
        [Column("update_time")]
        public DateTime update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// is_delete
        /// </summary>
        [Column("is_delete")]
        public bool is_delete { get; set; } = false;

        /// <summary>
        /// tenant_id
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;
    }
}
