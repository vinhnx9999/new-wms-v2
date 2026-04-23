using System.ComponentModel.DataAnnotations.Schema;

namespace WMSSolution.Core.Models.IntegrationWCS;

/// <summary>
/// Represents an outbound integration entity.
/// </summary>
[Table("IntegrationOutbounds")]
public class OutboundEntity : IntegrationEntity
{
    /// <summary>
    /// outbound gateway
    /// </summary>
    public int GatewayId { get; set; } = default!;
}
