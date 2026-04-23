namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Inbound;

/// <summary>
/// Gets or sets the unique identifier for the inbound task response.
/// </summary>
public class InboundTaskResponse
{
    /// <summary>
    /// Tasks the unique code that identifies the task. 
    /// </summary>
    public string TaskCode { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string AsnNumber { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string GoodOwnerName { get; set; } = "";

    /// <summary>
    /// Scheduled date and time for order pickup.
    /// </summary>
    public DateTime PickDate { get; set; }
    /// <summary>
    /// Gets or sets the name of the SKU (Stock Keeping Unit) associated with the inbound task.
    /// </summary>
    public string SkuCode { get; set; } = "";

    /// <summary>
    /// Gets or sets the collection of inbound task details associated with this instance.
    /// </summary>

    public IEnumerable<InboundTaskDTO> Details { get; set; } = [];

    /// <summary>
    /// Gets or sets the date and time when the response was generated, in Coordinated Universal Time (UTC).    
    /// </summary>
    public DateTime ResponseDate { get; set; } = DateTime.UtcNow;
}