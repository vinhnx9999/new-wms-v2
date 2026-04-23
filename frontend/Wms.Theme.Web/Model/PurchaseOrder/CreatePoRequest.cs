namespace Wms.Theme.Web.Model.PurchaseOrder
{
    public class CreatePoRequest
    {
        public string PoNo { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Description { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerAddress { get; set; }
        public string? PaymentTerm { get; set; }
        public decimal? ShippingAmount { get; set; }
        public List<CreatePoRequestDetail> Details { get; set; } = [];
    }

    public class CreatePoRequestDetail
    {
        public int SkuId { get; set; }
        public string? SkuName { get; set; }
        public string? SkuCode { get; set; }
        public int? SupplierId { get; set; }
        public decimal Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int SkuUomId { get; set; }
    }
}
