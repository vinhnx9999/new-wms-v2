namespace Wms.Theme.Web.Model.Reports;

public class LowStockAlertDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;

    public int SkuId { get; set; }
    public int UnitId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal SafetyStockQty { get; set; } = 0;
    public decimal Quantity { get; set; } = 0;
    public decimal IncomeQuantity { get; set; } = 0;
    public decimal OutcomeQuantity { get; set; } = 0;
    public decimal AdditionalQty
    { 
        get
        {
            return IncomeQuantity - OutcomeQuantity;
        }
            
    }
    public decimal BelowThreshold
    {
        get
        {
            if (Quantity > SafetyStockQty)
            {
                return SafetyStockQty - (Quantity + AdditionalQty);
            }

            return SafetyStockQty - Quantity;
        }
    }
}
