using System.ComponentModel;
using WMSSolution.Shared.Enums;

namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Outbound;

/// <summary>
/// DTO for getting outbound tasks by condition
/// </summary>
public class OutboundTaskConditionRequest
{
    /// <summary>
    /// Status filter
    /// </summary>
    public IntegrationStatus Status { get; set; }

    /// <summary>
    /// Date from filter
    /// </summary>
    [DefaultValue(null)]
    public DateTime? From { get; set; }

    /// <summary>
    /// Date to filter
    /// </summary>
    [DefaultValue(null)]
    public DateTime? To { get; set; }
}
