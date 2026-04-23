using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Shared.Enums;

namespace WMSSolution.Core.Models.IntegrationWCS;

/// <summary>
/// Swap Pallet
/// </summary>
[Table("IntegrationSwapPallets")]
public class SwapPalletEntity : GenericModel, ITenantEntity
{
    /// <summary>
    /// Priority
    /// </summary>
    public int Priority { get; set; } = 1;
    /// <summary>
    /// Tenant ID
    /// </summary>
    public long TenantId { get; set; } = 1;
    /// <summary>
    /// Pallet Code
    /// </summary>
    public string PalletCode { get; set; } = "";
    /// <summary>
    /// From Location
    /// </summary>
    public int FromLocationId { get; set; } = 1;
    /// <summary>
    /// To Location
    /// </summary>
    public int ToLocationId { get; set; } = 1;
    /// <summary>
    /// Status
    /// </summary>
    public IntegrationStatus Status { get; set; }
}