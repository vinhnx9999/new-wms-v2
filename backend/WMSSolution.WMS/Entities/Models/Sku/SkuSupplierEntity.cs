using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Sku
{
    /// <summary>
    /// N-N mapping table
    /// </summary>
    [Table("sku_supplier")]
    public class SkuSupplierEntity : BaseModel
    {
        /// <summary>
        /// skuId 
        /// </summary>
        [Column("sku_id")]
        public required int SkuId { get; set; }

        /// <summary>
        /// supplierId
        /// </summary>
        [Column("supplier_id")]
        public required int SupplierId { get; set; }

        /// <summary>
        /// Create time
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// update time
        /// </summary>
        [Column("update_time")]
        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is this the primary supplier for this SKU
        /// </summary>
        [Column("is_primary")]
        public bool IsPrimary { get; set; } = false;
    }
}
