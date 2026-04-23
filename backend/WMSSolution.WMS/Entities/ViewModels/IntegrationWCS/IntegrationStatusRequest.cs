using WMSSolution.Shared.Enums;

namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS;

/// <summary>
/// Represents a request to retrieve the integration status for a specific entity.
/// </summary>
public class IntegrationStatusRequest
{
    /// <summary>
    /// 
    /// </summary>
    public required string TaskCode { get; set; }
    /// <summary>
    /// Gets or sets the display name representing the current status.
    /// </summary>
    public IntegrationStatus Status { get; set; }
}
