namespace Wms.Theme.Web.Model.InboundReceipt;

public class InboundReceiptListResponse
{
    private string _dateFormat = "yyyy-MMM-dd";

    private string _dateTimeFormat = "yyyy-MMM-dd HH:mm";

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Receipt Number
    /// </summary>
    public required string ReceiptNo { get; set; }

    /// <summary>
    /// Receipt Type
    /// </summary>
    public string ReceiptType { get; set; } = string.Empty;

    /// <summary>
    /// Supplier Id
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// Supplier Name
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// Warehouse Id
    /// </summary>

    public int WarehouseId { get; set; }

    /// <summary>
    /// Warehouse Name
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// Status
    /// </summary>  
    public int Status { get; set; }

    /// <summary>
    /// Create date
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Total Qty Of This Inbound Receipt
    /// </summary>
    public int TotalQty { get; set; } = 0;

    /// <summary>
    /// Excetion flag, 
    /// </summary>
    public bool IsException { get; set; } = false;

    /// <summary>
    /// Is store 
    /// </summary>
    public bool? IsStored { get; set; } = true;

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Last Updated Date
    /// </summary>
    public DateTime? LastUpdatedDate { get; set; }
    public string StrDeletedDate
    {
        get
        {
            if(LastUpdatedDate == null) return string.Empty;

            try
            {
                DateTime localTime = LastUpdatedDate.GetValueOrDefault().ToLocalTime();
                return localTime.ToString(_dateTimeFormat);
            }
            catch
            {
                return "";
            }
        }
    }
    public string StrReceiptDate
    {
        get
        {
            try
            {
                DateTime localTime = CreateDate.ToLocalTime();
                return localTime.ToString(_dateFormat);
            }
            catch
            {
                return "";
            }
        }
    }
}

public class InboundItemResponse : InboundReceiptListResponse
{
    public int SkuId { get; set; }
    public string SkuName { get; set; } = string.Empty;
    public string SkuCode { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
}
