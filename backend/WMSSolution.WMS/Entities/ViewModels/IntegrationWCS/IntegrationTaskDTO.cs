namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS;

/// <summary>
/// Represents a data transfer object for an integration task, containing information about the task name, associated
/// pallet, scheduled pickup date, and location.
/// </summary>
public class IntegrationTaskDTO
{
    /// <summary>
    /// Gets or sets the unique code that identifies the pallet.
    /// Default is a series_number.
    /// </summary>
    public string PalletCode { get; set; } = "";

    /// <summary>
    /// Gets or sets the date and time when the item is scheduled to be picked up.
    /// </summary>
    public DateTime PickUpDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// 
    /// </summary>
    public string BlockId { get; set; } = "";

    /// <summary>
    /// Gets or sets the location information associated with this entity.
    /// </summary>
    public string Location { get; set; } = "";
    /// <summary>
    /// Gets or sets the current status message.
    /// </summary>

    public string? Status { get; set; }

}
