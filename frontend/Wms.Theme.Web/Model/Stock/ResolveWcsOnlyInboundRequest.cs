namespace Wms.Theme.Web.Model.Stock
{
    public class ResolveWcsOnlyInboundRequest
    {
        public int WarehouseId { get; set; }
        public string PalletCode { get; set; } = string.Empty;
        public string WcsLocation { get; set; } = string.Empty;
        public string? Note { get; set; }
        public List<ResolveWcsOnlyInboundItemRequest> Items { get; set; } = [];
    }

    public class ResolveWcsOnlyInboundItemRequest
    {
        public int SkuId { get; set; }
        public int Qty { get; set; }
        public int? SupplierId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? Price { get; set; }
    }
}
