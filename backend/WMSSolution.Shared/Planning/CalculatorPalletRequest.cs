namespace WMSSolution.Shared.Planning;

public class CalculatorPalletRequest
{
    public int? WarehouseId { get; set; }
    public IEnumerable<InboundDetailItem> Details { get; set; } = [];
}

public class AvailablePallet
{
    public string PalletName { get; set; } = "";
    public int LocationId { get; set; } = 0;
    public string LocationName { get; set; } = "";
    public int Quantity { get; set; } = 0;
    public int AvailableQty { get; set; } = 0;
    public int Balance { get; set; }
    public int SkuId { get; set; } = 0;
    public int? SkuUomId { get; set; } = 0;
    public string SkuName { get; set; } = "";
    public string SkuCode { get; set; } = "";
    public string UnitName { get; set; } = "";
}