
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.Models.Sku;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// sku_safety_stock entity
    /// </summary>
    [Table("sku_safety_stock")]
    public class SkuSafetyStockEntity : BaseModel
    {

        #region navigational properties
        /// <summary>
        /// navigational properties
        /// </summary>
        [ForeignKey("sku_id")]
        public SkuEntity Sku { get; set; }

        #endregion

        #region Property
        /// <summary>
        /// sku id
        /// </summary>
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// warehouse's id
        /// </summary>
        [Column("warehouse_id")]
        [JsonPropertyName("warehouse_id")]
        public int WarehouseId { get; set; } = 0;

        /// <summary>
        /// safety stock
        /// </summary>
        public int safety_stock_qty { get; set; } = 0;
        #endregion
    }
}
