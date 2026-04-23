namespace Wms.Theme.Web.Model.OutboundReceipt;

public class OutboundReceiptListResponse: BaseOutboundReceiptDto
{
    private string _dateTimeFormat = "yyyy-MMM-dd HH:mm";
    /// <summary>
    ///  id 
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// warehouse name 
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// customer name 
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// creator
    /// </summary>
    public string Creator { get; set; } = string.Empty;
    /// <summary>
    /// create date
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Outbound gateway name
    /// </summary>
    public string OutBoundGatewayName { get; set; } = string.Empty;

    /// <summary>
    /// Status of Outbound Receipt  
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Total Qty of Outbound Receipt
    /// </summary>
    public int TotalQty { get; set; } = 0;

    /// <summary>
    /// Receipt Date Or create date in string format for display, convert from UTC to local time
    /// </summary>
    public override string StrReceiptDate
    {
        get
        {
            try
            {
                var utcTime = ReceiptDate.GetValueOrDefault(CreateDate);
                DateTime localTime = utcTime.ToLocalTime();
                return localTime.ToString(_dateFormat);
            }
            catch
            {
                return "";
            }
        }
    }

    public DateTime? LastUpdatedDate { get; set; }
    public string StrDeletedDate
    {
        get
        {
            if (LastUpdatedDate == null) return string.Empty;

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
}
