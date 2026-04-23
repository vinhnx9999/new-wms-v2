namespace Wms.Theme.Web.Model.Stock
{
    public class ResolvePalletMergeSameLocationRequest
    {
        public int WarehouseId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? WmsPalletCode { get; set; }
        public string? WcsPalletCode { get; set; }
        public string TargetPalletCode { get; set; } = string.Empty;
        public string? Note { get; set; }
        public List<ResolvePalletMergeSameLocationItemRequest> Items { get; set; } = [];
    }

    public class ResolvePalletMergeSameLocationItemRequest
    {
        public int? SkuId { get; set; }
        public int? SupplierId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? Qty { get; set; }
    }
}