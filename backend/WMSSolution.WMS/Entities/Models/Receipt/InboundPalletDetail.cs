using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Receipt
{
    /// <summary>
    /// Details
    /// </summary>
    [Table("inbound_pallet_detail")]
    public class InboundPalletDetail : BaseModel
    {
        /// <summary>
        /// Sku id 
        /// </summary>
        [Column("sku_id")]
        public int SkuId { get; set; }

        /// <summary>
        /// SkuOumId 
        /// </summary>
        [Column("sku_uom_id")]
        public int SkuUomId { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        [Column("quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Supplier Id 
        /// </summary>
        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        /// <summary>
        /// Exp
        /// </summary>
        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// inbound pallet id 
        /// </summary>
        [Column("inbound_pallet_id")]
        [ForeignKey(nameof(InboundPallet))]
        public int InboundPalletId { get; set; }

        /// <summary>
        /// Foreign key to pallet
        /// </summary>
        public InboundPallet InboundPallet { get; set; } = default!;

    }
}
