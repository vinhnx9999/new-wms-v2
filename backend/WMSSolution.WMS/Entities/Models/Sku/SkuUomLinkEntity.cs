using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Sku
{
    /// <summary>
    /// mapping n-n between sku and sku uom
    /// </summary>
    [Table("sku_uom_link")]
    public class SkuUomLinkEntity : BaseModel
    {
        /// <summary>
        /// sku id
        /// </summary>
        [Column("sku_id")]
        public required int SkuId { get; set; }
        /// <summary>
        /// sku uom id
        /// </summary>  
        [Column("sku_uom_id")]
        public required int SkuUomId { get; set; }

        /// <summary>
        ///  Is base unit
        ///  default : not is base unit 
        /// </summary>
        [Column("is_base_unit")]
        public bool IsBaseUnit { get; set; } = false;

        /// <summary>
        /// Conversion Rate  
        /// 1 UnitName A = 2 UnitName B with A is base unit
        /// </summary>
        [Column("conversion_rate")]
        public int ConversionRate { get; set; } = 1;
        /// <summary>
        /// SkuId
        /// </summary>
        [ForeignKey(nameof(SkuId))]
        public virtual SkuEntity? Sku { get; set; } = null;
        /// <summary>
        /// SkuOum
        /// </summary>
        [ForeignKey(nameof(SkuUomId))]
        public virtual SkuUomEntity? SkuUom { get; set; } = null;
    }
}
