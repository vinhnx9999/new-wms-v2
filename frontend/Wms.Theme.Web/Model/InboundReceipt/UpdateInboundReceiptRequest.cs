namespace Wms.Theme.Web.Model.InboundReceipt
{
    public class UpdateInboundReceiptRequest
    {
        /// <summary>
        /// Receipt Number
        /// </summary>
        public string ReceiptNo { get; set; } = string.Empty;

        /// <summary>
        /// Warehouse ID
        /// </summary>
        public int WarehouseId { get; set; }

        /// <summary>
        /// Type of receipt 
        /// </summary>
        public string? Type { get; set; }
        /// <summary>
        /// Is Store in  Stock
        /// </summary>
        public bool IsStored { get; set; } = true;

        /// <summary>
        /// Description of this Receipt
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Create date 
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        /// <summary>
        /// List of receipt details
        /// </summary>
        public List<UpdateReceiptDetailDto> Details { get; set; } = [];

        /// <summary>
        /// supplier id  
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// Upgrate status from draft to new
        /// </summary>
        public bool IsUpgradeStatus { get; set; } = false;

        /// <summary>
        /// Deliverer
        /// </summary>
        public string? Deliverer { get; set; }

        /// <summary>
        /// invoice number
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Expected delivery date
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }
        public bool MultiPallets { get; set; } = false;
    }

    /// <summary>
    /// Detail item for receipt update
    /// </summary>
    public class UpdateReceiptDetailDto
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; } = 0;
        /// <summary>
        /// SKU ID
        /// </summary>
        public int SkuId { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit of Measure ID
        /// </summary>
        public int SkuUomId { get; set; }

        /// <summary>
        /// Goods Location ID (Stock Location)
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Pallet Code
        /// </summary>
        public string? PalletCode { get; set; }

        /// <summary>
        /// Expire Date
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Inbound Order Ref
        /// </summary>
        public int? AsnId { get; set; }

        /// <summary>
        /// Source number
        /// </summary>
        public string? SourceNumber { get; set; }
    }
}
