using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Shared.Enums;

namespace WMSSolution.Core.Models.IntegrationWCS;

/// <summary>
/// Represents a record of an integration event or operation for auditing or tracking purposes.
/// </summary>
/// <remarks>This class is typically used to store historical information about integration processes, such as
/// data exchanges or synchronization events, within the application. It inherits from <see cref="IntegrationEntity"/>,
/// which may provide additional properties relevant to integration tracking.</remarks>
[Table("IntegrationHistories")]
public class IntegrationHistory : IntegrationEntity
{
    /// <summary>
    /// HistoryType
    /// </summary>
    public HistoryType HistoryType { get; set; }

    /// <summary>
    /// Description for inbound
    /// </summary>
    public string? Description { get; set; }
}