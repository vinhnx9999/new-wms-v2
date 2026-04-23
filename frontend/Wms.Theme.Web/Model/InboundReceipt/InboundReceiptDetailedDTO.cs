namespace Wms.Theme.Web.Model.InboundReceipt
{
    /// <summary>
    /// Detailed DTO for a single inbound receipt with items
    /// </summary>
    public class InboundReceiptDetailedDTO
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Receipt Number
        /// </summary>
        public string ReceiptNo { get; set; } = string.Empty;

        /// <summary>
        /// Warehouse ID
        /// </summary>
        public int WarehouseId { get; set; }

        /// <summary>
        /// Warehouse Name
        /// </summary>  
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// Type of receipt (default: Inbound)
        /// </summary>
        public string Type { get; set; } = String.Empty;

        /// <summary>
        /// Is Store in  Stock
        /// </summary>
        public bool IsStored { get; set; }

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
        public List<InboundReceiptDetailItemDTO> Details { get; set; } = [];

        /// <summary>
        /// supplier id  
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// Supplier Name
        /// </summary>
        public string SupplierName { get; set; } = string.Empty;

        /// <summary>
        /// Status of the receipt
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// invoice number
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// deliver name
        /// </summary>
        public string? Deliverer { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Expected delivery date
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }
        public bool? MultiPallets { get; set; } = false;
        public string SharingUrl { get; set; } = "";
    }

    /// <summary>
    /// Detail item within a receipt
    /// </summary>
    public class InboundReceiptDetailItemDTO
    {
        /// <summary>
        /// Id of Detail Item
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// SKU ID
        /// </summary>
        public int SkuId { get; set; }

        /// <summary>
        /// Sku Code
        /// </summary>
        public string SkuCode { get; set; } = string.Empty;

        /// <summary>
        /// Sku Name
        /// </summary>
        public string SkuName { get; set; } = string.Empty;

        /// <summary>
        /// Quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit of Measure ID
        /// </summary>
        public int SkuUomId { get; set; }

        /// <summary>
        /// Unit name
        /// </summary>
        public string UnitName { get; set; } = string.Empty;

        /// <summary>
        /// Goods Location ID (Stock Location)
        /// </summary>
        public int? LocationId { get; set; }

        // location Name
        public string? LocationName { get; set; }
        /// <summary>
        /// Pallet Code
        /// </summary>
        public string? PalletCode { get; set; }

        /// <summary>
        /// Expire Date
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Is Exception
        /// </summary>
        public bool IsException { get; set; } = false;

        /// <summary>
        /// Source number
        /// </summary>
        public string? SourceNumber { get; set; }
    }
}
