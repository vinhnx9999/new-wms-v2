namespace WMSSolution.Shared.Planning;

public class InboundDetailItem
{
    public int SkuId { get; set; } = 0;
    public int Quantity { get; set; } = 0;
    public int? SkuUomId { get; set; } = 0;
    public int? SupplierId { get; set; }
    public DateTime? ExpiryDate { get; set; }
}