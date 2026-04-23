namespace WMSSolution.WMS.Entities.ViewModels.PurchaseOrders
{
    /// <summary>
    /// Create Request for new PO
    /// </summary>
    public class CreateNewPoRequest
    {
        /// <summary>
        /// Po number
        /// </summary>
        public string PoNo { get; set; } = default!;

        /// <summary>
        /// ref supplier id
        /// </summary>

        public int? SupplierId { get; set; }

        /// <summary>
        /// supplier name
        /// </summary>
        public string? SupplierName { get; set; }

        /// <summary>
        /// order date
        /// </summary>
        public DateTime OrderDate { get; set; } = default!;

        /// <summary>
        /// expected delivery date
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }


        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// buyer name
        /// </summary>
        public string? BuyerName { get; set; }

        /// <summary>
        /// buyer address
        /// </summary>
        public string? BuyerAddress { get; set; }

        /// <summary>
        /// Payment term
        /// </summary>
        public string? PaymentTerm { get; set; }

        /// <summary>
        /// Shipping amount
        /// </summary>
        public decimal? ShippingAmount { get; set; }

        /// <summary>
        /// Details
        /// </summary>
        public List<CreateNewPoRequestDetails> Details { get; set; } = default!;

    }

    /// <summary>
    /// Details of creating new PO
    /// </summary>
    public class CreateNewPoRequestDetails
    {
        /// <summary>
        /// sku_id
        /// </summary>
        public int SkuId { get; set; } = default!;

        /// <summary>
        /// sku name
        /// </summary>
        public string? SkuName { get; set; }

        /// <summary>
        /// sku_code
        /// </summary>
        public string? SkuCode { get; set; }

        /// <summary>
        /// SupplierId
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// Qty ordered
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// unit price
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// exp of this item
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Sku Oum id 
        /// </summary>
        public int SkuUomId { get; set; } = default!;

    }
}
