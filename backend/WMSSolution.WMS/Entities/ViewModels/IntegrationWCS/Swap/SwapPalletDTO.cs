namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Swap;

/// <summary>
/// Swap Pallet DTO
/// </summary>
public class SwapPalletDTO
{
    /// <summary>
    /// Pallet Code
    /// </summary>
    public string PalletCode { get; set; } = string.Empty;
    /// <summary>
    /// From Location
    /// </summary>
    public string FromLocation { get; set; } = string.Empty;
    /// <summary>
    /// To Location
    /// </summary>
    public string ToLocation { get; set; } = string.Empty;
    /// <summary>
    /// Status
    /// </summary>
    public string Status { get;  set; } = string.Empty;
    /// <summary>
    /// Swap Id
    /// </summary>
    public long SwapId { get; set; }
}
