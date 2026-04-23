using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Sku
{
    /// <summary>
    /// Sku Specification entity
    /// </summary>
    [Table("sku_specification")]
    public class SkuSpecificationEntity : BaseModel
    {
        #region Properties
        /// <summary>
        /// sku_id
        /// </summary>
        [Column("sku_id")]
        public int sku_id { get; set; }

        /// <summary>
        /// spu_id
        /// </summary>
        [Column("specification_id")]
        public int specification_id { get; set; }

        /// <summary>
        /// SkuId
        /// </summary>
        [ForeignKey(nameof(sku_id))]
        public virtual SkuEntity? Sku { get; set; } = null;

        /// <summary>
        /// SkuOum
        /// </summary>
        [ForeignKey(nameof(specification_id))]
        public virtual SpecificationEntity? Specification { get; set; } = null;
        #endregion
    }
}
