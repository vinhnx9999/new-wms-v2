using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Model.Reports;

public class BaseTransactionReport
{
    public int? ReceiptId { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public decimal? InwardQuantity { get; set; } = 0;
    public decimal? OutwardQuantity { get; set; } = 0;
    /// <summary>
    /// Ngày phát sinh nghiệp vụ
    /// </summary>
    public DateTime? TransactionDate { get; set; }
    public string? Description { get; set; } = "";
    public string TransDate
    {
        get
        {
            if (!TransactionDate.HasValue || TransactionDate.GetValueOrDefault().Year < 2000) return "";
            return TransactionDate.GetValueOrDefault().Convert2LocalDate();
        }
    }

    public string ReceiptDateStr
    {
        get
        {
            if (!ReceiptDate.HasValue || ReceiptDate.GetValueOrDefault().Year < 2000) return "";
            return ReceiptDate.GetValueOrDefault().Convert2LocalDate();
        }
    }

}
public class InOutStatementDto : BaseTransactionReport
{
    public string SerialNumber { get; set; } = "";
    public int SkuId { get; set; }
    public int UnitId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

}

public class InventoryCardItem : BaseTransactionReport
{
    public decimal? ClosedBalance { get; set; } = 0;
    /// <summary>
    /// Warehouse Receipt - GRN: Chứng từ nhập
    /// </summary>
    public string? GoodsReceiptNote { get; set; } = "";
    /// <summary>
    /// Delivery Note - GIN: Chứng từ xuất
    /// </summary>
    public string? GoodsIssueNote { get; set; } = "";
}

public class InventoryReportItem
{
    public string WarehouseName { get; set; } = string.Empty;
    public int SkuId { get; set; }
    public int? UnitId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; } = 0;
    public decimal InwardQuantity { get; set; } = 0;
    public decimal OutwardQuantity { get; set; } = 0;

    public decimal OpeningCost { get; set; } = 0;
    public decimal InwardCost { get; set; } = 0;
    public decimal OutwardCost { get; set; } = 0;

    public decimal ClosedBalance
    {
        get
        {
            return OpeningBalance + InwardQuantity - OutwardQuantity;
        }
    }

    public decimal OpeningValue
    {
        get
        {
            return OpeningBalance * OpeningCost;
        }
    }

    public decimal InwardValue
    {
        get
        {
            return InwardQuantity * InwardCost;
        }
    }

    public decimal OutwardValue
    {
        get
        {
            return OutwardQuantity * OutwardCost;
        }
    }

    public decimal ClosedValue
    {
        get
        {
            return OpeningValue + InwardValue - OutwardValue;
        }
    }
}
