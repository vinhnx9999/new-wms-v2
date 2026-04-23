namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Outbound;
/// <summary>
/// Presenting a request to update the status of an outbound task.
/// </summary>
public class OutboundStatusRequest
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
