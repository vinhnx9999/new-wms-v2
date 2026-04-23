using WMSSolution.Shared.Enums;

namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Swap;

/// <summary>
/// Swap Pallets
/// </summary>
public class ReshufflingResponse
{
    /// <summary>
    /// Swap Pallet Info
    /// </summary>
    public IEnumerable<SwapPalletDTO> Details { get; set; } = [];

    /// <summary>
    /// Gets or sets the date and time when the response was generated, in Coordinated Universal Time (UTC).    
    /// </summary>
    public DateTime ResponseDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Update Reshuffling
/// </summary>
public class ReshufflingRequest
{
    /// <summary>
    /// Swap Id
    /// </summary>
    public long SwapId { get; set; } = 1;
    /// <summary>
    /// Gets or sets the display name representing the current status.
    /// </summary>
    public IntegrationStatus Status { get; set; }
}