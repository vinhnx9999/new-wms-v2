using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Sku
{
    /// <summary>
    /// Stock Keeping Unit - Unit of Measure
    /// </summary>
    [Table("sku_uom")]
    public class SkuUomEntity : BaseModel, ITenantEntity
    {
        /// <summary>
        /// UnitName
        /// </summary>
        public required string UnitName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// tenant_id
        /// </summary>
        public long TenantId { get; set; }

        /// <summary>
        /// Sku Oum links
        /// </summary>
        public virtual List<SkuUomLinkEntity> SkuUomLinks { get; set; } = [];
    }
}
