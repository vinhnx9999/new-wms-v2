using Mapster;
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums;

namespace WMSSolution.WMS.Entities.Models.PurchaseOrders
{
    /// <summary>
    /// Purchase Order Entity for InBound Process
    /// </summary>
    [Table("purchaseorders")]
    public class PurchaseOrderEntity : BaseModel, ITenantEntity
    {
        /// <summary>
        /// Po number
        /// </summary>
        [Column("po_no")]
        [AdaptMember("po_no")]
        public string PoNo { get; set; } = string.Empty;

        /// <summary>
        /// ref supplier id
        /// </summary>
        /// <remarks> 
        /// if this po has mutil suppliers, 
        /// set the first supplier id in details
        /// </remarks>
        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        /// <summary>
        /// Suppliername
        /// </summary>
        [Column("supplier_name")]
        public string? SupplierName { get; set; }

        /// <summary>
        /// order date
        /// </summary>
        [Column("order_date")]
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// expected delivery date
        /// </summary>
        [Column("expected_delivery_date")]
        public DateTime? ExpectedDeliveryDate { get; set; }
        /// <summary>
        /// po status 
        /// @1 = new , @2 = processing , @3 completed , @4 canceled
        ///</summary>
        [Column("po_status")]
        public PoStatusEnum PoStatus { get; set; } = PoStatusEnum.CREATED;

        /// <summary>
        /// Create time
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// update time
        /// </summary>
        [Column("last_update_time")]
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// staff name
        /// </summary>
        [Column("creator")]
        public string Creator { get; set; } = default!;

        /// <summary>
        /// buyer
        /// </summary>
        [Column("buyer_name")]
        public string? BuyerName { get; set; }

        /// <summary>
        /// buyer address
        /// </summary>
        [Column("buyer_address")]
        public string? BuyerAddress { get; set; }

        /// <summary>
        /// Payment term
        /// </summary>
        [Column("payment_term")]
        public string? PaymentTerm { get; set; }

        /// <summary>
        /// is multi supplier 
        /// </summary>
        [Column("is_multi_supplier")]
        public bool IsMultiSupplier { get; set; } = false;

        /// <summary>
        /// Description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Count supplier
        /// </summary>
        [NotMapped]
        public int CountSupplier => Details?.Select(d => d.SupplierId).Distinct().Count() ?? 0;

        /// <summary>
        /// Shipping amount
        /// </summary>
        [Column("shipping_amount")]
        public decimal? ShippingAmount { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        [Column("total_amount")]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Navigation property name
        /// </summary>
        [ForeignKey(nameof(PurchaseOrderDetailsEntity.PoId))]
        public List<PurchaseOrderDetailsEntity> Details { get; set; } = [];

        /// <summary>
        /// tenant id 
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;
    }
}