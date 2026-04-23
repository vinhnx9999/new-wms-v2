namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;


/// <summary>
/// Rule Settings
/// </summary>
public class InventoryRuleSettings
{
    /// <summary>
    /// Rule Name
    /// </summary>
    public string RuleName { get; set; } = "";
    /// <summary>
    /// Rule Settings FIFO: First In First Out, FEFO: First Expire First Out, LIFO: Last In First Out
    /// </summary>
    public OperationRules RuleSettings { get; set; } = OperationRules.FEFO;
    /// <summary>
    /// WarehouseId
    /// </summary>
    public int WarehouseId { get; set; } = 1;
    /// <summary>
    /// Suppliers
    /// </summary>
    public IEnumerable<int> SupplierIds { get; set; } = [];
    /// <summary>
    /// Details
    /// </summary>
    public IEnumerable<RuleDetail> Details { get; set; } = [];
    /// <summary>
    /// Created Date
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Details
/// </summary>
public class RuleDetail
{
    /// <summary>
    /// Block
    /// </summary>
    public int? BlockId { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the category associated with the item.
    /// </summary>
    /// <remarks>The value can be null, indicating that the item is not assigned to any category. Set this
    /// property to associate the item with a specific category or leave it unset to indicate no association.</remarks>
    public int? CategoryId { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the floor associated with the entity.
    /// </summary>
    /// <remarks>The value can be null, indicating that the entity is not assigned to any specific floor.
    /// Check for null before performing operations that require a valid floor identifier.</remarks>
    public int? FloorId { get; set; }
    /// <summary>
    /// Sku
    /// </summary>
    public int? SkuId { get; set; }
}
