namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Outbound;

/// <summary>
/// Gets or sets the unique identifier for the outbound task response.
/// </summary>
public class OutboundTaskResponse
{
    /// <summary>
    /// Tasks the unique code that identifies the task. 
    /// </summary>
    public string TaskCode { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string SoNumber { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string CustomerName { get; set; } = default!;

    /// <summary>
    /// Gateway name
    /// </summary>
    public string GatewayName { get; set; } = default!;

    /// <summary>
    /// Scheduled date and time for order pickup.
    /// </summary>
    public DateTime PickDate { get; set; }
    /// <summary>
    /// Gets or sets the name of the SKU (Stock Keeping Unit) associated with the inbound task.
    /// </summary>
    public string SkuCode { get; set; } = "";

    /// <summary>
    /// Gets or sets the collection of outbound task details associated with this instance.
    /// </summary>
    public IEnumerable<OutboundTaskDTO> Details { get; set; } = [];

    /// <summary>
    /// Gets or sets the date and time when the response was generated, in Coordinated Universal Time (UTC).    
    /// </summary>
    public DateTime ResponseDate { get; set; } = DateTime.UtcNow;
}
