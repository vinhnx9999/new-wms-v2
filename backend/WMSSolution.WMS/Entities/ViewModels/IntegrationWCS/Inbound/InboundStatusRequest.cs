namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Inbound;

/// <summary>
/// Represents a request to retrieve the status of an inbound integration operation.
/// </summary>
public class InboundStatusRequest
{
    /// <summary>
    /// Task Code of the inbound operation.
    /// </summary>
    public required string TaskCode { get; set; } = "";
    /// <summary>
    /// Pallet Code
    /// </summary>
    public required string PalletCode { get; set; } = "";

}
