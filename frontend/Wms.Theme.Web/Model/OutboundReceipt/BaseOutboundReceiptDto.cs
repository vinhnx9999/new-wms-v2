namespace Wms.Theme.Web.Model.OutboundReceipt;

public class BaseOutboundDetailDto
{
    /// <summary>
    /// SKU ID
    /// </summary>
    public int SkuId { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit of Measure ID
    /// </summary>
    public int SkuUomId { get; set; }

    /// <summary>
    /// Goods Location ID (Stock Location)
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// Pallet Code
    /// </summary>
    public string? PalletCode { get; set; }
}

public class BaseOutboundReceiptDto
{
    /// <summary>
    /// Format date time to string
    /// </summary>
    protected const string _dateFormat = "yyyy-MMM-dd";

    /// <summary>
    /// Receipt Number
    /// </summary>
    public string ReceiptNo { get; set; } = string.Empty;

    /// <summary>
    /// Warehouse ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// Outbound gateway ID
    /// </summary>
    public int OutboundGatewayId { get; set; }

    /// <summary>
    /// Customer ID
    /// </summary>
    public int CustomerId { get; set; }

    public string? Consignee { get; set; }
    /// <summary>
    /// Type of receipt (default: Outbound)
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// Create date `
    /// </summary>
    public DateTime? ReceiptDate { get; set; }
    public DateTime? ExpectedShipDate { get; set; }
    public DateTime? StartPickingTime { get; set; }

    public virtual string StrReceiptDate
    {
        get
        {                       
            if (ReceiptDate.HasValue)
            {
                var utcTime = ReceiptDate.Value;
                DateTime localTime = utcTime.ToLocalTime();
                return localTime.ToString(_dateFormat);
            }

            return "";
        }
    }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }
}