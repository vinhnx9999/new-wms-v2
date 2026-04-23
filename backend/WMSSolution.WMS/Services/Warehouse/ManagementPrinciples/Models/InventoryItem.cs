namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

/// <summary>
/// Represents an item in the inventory, including details such as name, category, quantity, and relevant dates for
/// receipt and expiration.
/// </summary>
/// <remarks>Use this class to track inventory items, monitor stock levels, and manage expiration and reorder
/// information. The properties provide essential data for inventory management operations, such as restocking and
/// auditing.</remarks>
public class InventoryItem
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// Category
    /// </summary>
    public string Category { get; set; } = "";
    /// <summary>
    /// Quantity
    /// </summary>
    public int Quantity { get; set; } = 0;
    /// <summary>
    /// Gets or sets the date and time when the item was received.
    /// </summary>
    /// <remarks>The value is initialized to the current UTC date and time when the instance is
    /// created.</remarks>
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Gets or sets the expiration date and time for the item.
    /// </summary>
    /// <remarks>Use this property to determine whether the item is still valid or has expired. The value is
    /// typically compared to the current date and time to enforce expiration logic.</remarks>
    public DateTime ExpirationDate { get; set; }   
    /// <summary>
    /// Gets or sets the minimum quantity of an item that must be in stock before a reorder is triggered.
    /// </summary>
    /// <remarks>Set this property to ensure inventory is replenished before reaching critically low levels.
    /// Adjusting the reorder level appropriately helps prevent stockouts and maintain optimal inventory
    /// levels.</remarks>
    public int ReorderLevel { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier for the supplier.
    /// </summary>
    public int SupplierId { get; set; }
    /// <summary>
    /// Gets or sets the supplier associated with this entity.
    /// </summary>
    /// <remarks>Use this property to retrieve or assign supplier information relevant to the entity. The
    /// supplier may be used for managing supplier-related operations within the context of inventory or warehouse
    /// management.</remarks>
    public Supplier Supplier { get; set; } = new Supplier();
}



