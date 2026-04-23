namespace Wms.Theme.Web.Model.OutboundReceipt;

public class CompanyInfo
{
    public string CompanyName { get; set; } = "CÔNG TY TNHH SX – TM – DV – KT DUY PHÁT";
    public string Address { get; set; } = "106/6 Ngô Tất Tố, Phường Thạnh Mỹ Tây, TP Hồ Chí Minh, Việt Nam";
}

public class OutboundReceiptDetailedDto: BaseOutboundReceiptDto
{
    /// <summary>
    /// id of the receipt
    /// </summary>
    public int Id { get; set; }
   
    /// <summary>
    /// Warehouse Name
    /// </summary>  
    public string WarehouseName { get; set; } = string.Empty;

    public string WarehouseAddress { get; set; } = string.Empty;

    /// <summary>
    /// Create date 
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// List of receipt details
    /// </summary>
    public List<OutboundReceiptDetailItemDto> Details { get; set; } = [];

    /// <summary>
    /// Customer Name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Outbound gateway name
    /// </summary>
    public string OutboundGatewayName { get; set; } = default!;
    public string SharingUrl { get; set; } = string.Empty;
    /// <summary>
    /// Status of the receipt
    /// </summary>
    public int Status { get; set; }

    public override string StrReceiptDate
    {
        get
        {
            try
            {
                if (ReceiptDate == null && CreatedDate == null)
                {
                    return "";
                }

                var utcTime = ReceiptDate.GetValueOrDefault(CreatedDate.GetValueOrDefault());
                DateTime localTime = utcTime.ToLocalTime();
                return localTime.ToString(_dateFormat);
            }
            catch
            {
                return "";
            }
        }

    }
}

/// <summary>
/// Detail item within a receipt
/// </summary>
public class OutboundReceiptDetailItemDto: BaseOutboundDetailDto
{
    /// <summary>
    /// Id of Details
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Sku Code
    /// </summary>
    public string SkuCode { get; set; } = string.Empty;
    /// <summary>
    /// Sku Name
    /// </summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>
    /// Unit name
    /// </summary>
    public string UnitName { get; set; } = string.Empty;
    /// <summary>
    /// LocationName 
    /// </summary>
    public string? LocationName { get; set; }
    /// <summary>
    /// Is Exception
    /// </summary>
    public bool IsException { get; set; } = false;

}
