using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.PurchaseOrders
{
    /// <summary>
    /// purchase order details entity
    /// </summary>
    [Table("purchaseorderdetails")]
    public class PurchaseOrderDetailsEntity : BaseModel
    {
        /// <summary>
        /// sku id
        /// </summary>
        [Column("sku_id")]
        public int SkuId { get; set; }
        /// <summary>
        /// sku name
        /// </summary>
        [Column("sku_name")]
        public string? SkuName { get; set; }
        /// <summary>
        /// spu id
        /// </summary>
        [JsonPropertyName("spu_id")]
        [Column("spu_id")]
        public int SpuId { get; set; } = 0;

        /// <summary>
        /// spu name
        /// </summary>
        [Column("spu_name")]
        public string? SpuName { get; set; }

        /// <summary>
        /// SupplierId
        /// </summary>
        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        /// <summary>
        /// Supplier Name
        /// </summary>
        [Column("supplier_name")]
        public string? SupplierName { get; set; }

        /// <summary>
        /// qty ordered
        /// </summary>
        [Column("qty_ordered")]
        public decimal QtyOrdered { get; set; }

        /// <summary>
        /// qty received actually
        /// </summary>
        [Column("qty_received")]
        public decimal QtyReceived { get; set; }

        /// <summary>
        /// the quantity still miss
        /// </summary>
        [NotMapped]
        public decimal QtyOpen => QtyOrdered - QtyReceived;

        /// <summary>
        /// unit price
        /// </summary>
        [Column("unit_price")]
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Sku uom id 
        /// </summary>
        [Column("sku_uom_id")]
        public int SkuUomId { get; set; }

        /// <summary>
        /// id Fk
        /// </summary>
        [Column("po_id")]
        public int PoId { get; set; }

        /// <summary>
        /// exp of this item
        /// </summary>
        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }
    }
}
