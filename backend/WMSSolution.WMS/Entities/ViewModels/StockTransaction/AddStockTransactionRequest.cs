using WMSSolution.WMS.Entities.Models.Stock;

namespace WMSSolution.WMS.Entities.ViewModels.StockTransaction
{
    /// <summary>
    /// Record to add stock transaction
    /// </summary>
    /// <param name="StockId"></param>
    /// <param name="QuantityChange"></param>
    /// <param name="SkuId"></param>
    /// <param name="TransactionType"></param>
    /// <param name="TenantId"></param>
    /// <param name="SkuUomId"></param>
    /// <param name="ConversionRate"></param>
    /// <param name="UnitName"></param>
    /// <param name="RefReceipt"></param>
    /// <param name="SupplierId"></param>
    /// <param name="SupplierName"></param>
    /// <param name="CustomerId"></param>
    /// <param name="CustomerName"  ></param>
    public record AddStockTransactionRequest(
    int StockId,
    decimal QuantityChange,
    int SkuId,
    StockTransactionType TransactionType,
    long TenantId,
    int? SkuUomId = null,
    int? ConversionRate = null,
    string? UnitName = null,
    string? RefReceipt = null,
    int? SupplierId = null,
    string? SupplierName = null,
    int? CustomerId = null,
    string? CustomerName = null);
}
