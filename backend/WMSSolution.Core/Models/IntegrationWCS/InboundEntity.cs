using System.ComponentModel.DataAnnotations.Schema;

namespace WMSSolution.Core.Models.IntegrationWCS;

/// <summary>
/// Represents an entity that is received from an external source and is associated with a tenant context.
/// </summary>
[Table("IntegrationInbounds")]
public class InboundEntity : IntegrationEntity
{
   
}
