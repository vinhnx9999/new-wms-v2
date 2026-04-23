using WMSSolution.WMS.Entities.Models.Warehouse;

namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

/// <summary>
/// Represents a set of rules that govern warehouse inventory management and configuration.
/// </summary>
/// <remarks>Use this class to encapsulate the unique identifier and the collection of rule settings that define
/// operational or configuration criteria for warehouse management. The rules contained within this class determine how
/// inventory is managed and controlled within the warehouse context.</remarks>
public class InventoryRules 
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the collection of rules that define the warehouse settings.
    /// </summary>
    /// <remarks>Each rule in the collection specifies configuration or operational criteria for warehouse
    /// management. Use this property to retrieve or assign the set of rules that govern warehouse behavior.</remarks>
    public IEnumerable<WarehouseRuleSettingsEntity> Rules { get; set; } = [];
}


