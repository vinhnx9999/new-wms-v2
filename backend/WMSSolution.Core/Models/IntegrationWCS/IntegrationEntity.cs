using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Shared.Enums;

namespace WMSSolution.Core.Models.IntegrationWCS;

/// <summary>
/// Represents an integration entity containing information about a pallet, its pickup schedule, location, and
/// integration status within a tenant context.
/// </summary>
public class IntegrationEntity : GenericModel, ITenantEntity
{
    // public string Task
    /// <summary>
    /// Gets or sets the unique code that identifies the pallet.
    /// </summary>
    public string PalletCode { get; set; } = "";

    /// <summary>
    /// Gets or sets the scheduled date and time for order pickup.
    /// </summary>
    public DateTime? PickUpDate { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier for the location.
    /// </summary>
    public int LocationId { get; set; } = 0;
    /// <summary>
    /// Gets or sets the priority level for the associated operation or item.
    /// </summary>
    public int Priority { get; set; } = 1;
    /// <summary>
    /// TenantId
    /// </summary>
    public long TenantId { get; set; } = 1;
    /// <summary>
    /// Gets or sets the current integration status.
    /// </summary>
    public IntegrationStatus Status { get; set; } = IntegrationStatus.Ready;
    /// <summary>
    /// Gets or sets the date and time when the operation was completed.
    /// </summary>
    public DateTime? FinishedDate { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Gets or sets a value indicating whether the object is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Wcs key
    /// </summary>
    [Column("wcs_key")]
    public string? WcsKey { get; set; }
}
