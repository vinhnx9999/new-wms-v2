using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Utility;
using WMSSolution.Shared.Enums;
namespace WMSSolution.Core.Models;

/// <summary>
/// Serves as a base class for data models that require a unique integer identifier.
/// </summary>
/// <remarks>This abstract class provides a standard integer primary key property for derived model classes. It is
/// intended to be inherited by entity types that are persisted to a data store and require a unique
/// identifier.</remarks>
[Serializable]
public abstract class BaseModel
{
    /// <summary>
    /// id
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; } = 0;

}

/// <summary>
/// Represents an entity that is associated with a tenant in a multi-tenant system.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// tenant_id
    /// </summary>
    [Column("tenant_id")]
    long TenantId { get; set; }
}

/// <summary>
/// Represents a base class for models that include a unique integer identifier.
/// </summary>
/// <remarks>This abstract class is intended to be inherited by entity types that require a primary key property
/// named <c>id</c>. The <c>id</c> property is marked with the <see
/// cref="System.ComponentModel.DataAnnotations.KeyAttribute"/> to indicate its use as a primary key in data
/// contexts.</remarks>
[Serializable]
public abstract class GenericModel
{
    /// <summary>
    /// id
    /// </summary>
    [Key]
    public long Id { get; set; } = 0;

    /// <summary>
    /// TaskCode
    /// Auto generated
    /// </summary>
    public string TaskCode { get; set; } = GenarationHelper.GenerateTaskCode();
}