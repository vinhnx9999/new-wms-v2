namespace WMSSolution.Shared.Excel;

public class BaseOrderExcel
{
    public string OrderCode { get; set; } = string.Empty;
    public string WareHouseName { get; set; } = string.Empty;    
    public string SkuCode { get; set; } = string.Empty;
    public string Qty { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class InboundOrderExcel: BaseOrderExcel
{
    public string OrderType { get; set; } = "PURCHASE";
    public string SupplierName { get; set; } = string.Empty;
    public DateTime? ExpireDate { get; set; } = DateTime.UtcNow.AddDays(15);
    public string IsPutaway { get; set; } = string.Empty;
}

public class BeginMerchandiseExcel : BaseOrderExcel
{
    public string SupplierName { get; set; } = string.Empty;
    public DateTime? ExpireDate { get; set; } = DateTime.UtcNow.AddDays(15);
    public string IsPutaway { get; set; } = string.Empty;
}

public class OutboundOrderExcel : BaseOrderExcel
{
    public string OrderType { get; set; } = "SALES";
    public string CustomerName { get; set; } = string.Empty;
    public string GatewayName { get; set; } = string.Empty;
}