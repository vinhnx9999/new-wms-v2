namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

/// <summary>
/// Search Condition
/// </summary>
public class SearchCondition
{
    /// <summary>
    /// Gets or sets the category of the item, which can be used to group or classify it within a specific context.
    /// </summary>
    public string Category { get; set; } = "";
    /// <summary>
    /// Gets or sets the quantity of items to be ordered.
    /// </summary>
    /// <remarks>The order quantity must be a positive integer. This property influences the total cost of the
    /// order and may affect inventory levels.</remarks>
    public int OrderQuantity { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier for the supplier.
    /// </summary>
    public int? SupplierId { get; set; }
}
