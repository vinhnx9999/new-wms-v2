namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

/// <summary>
/// Represents a record of a change in inventory for a specific item, including the quantity affected and the type of
/// transaction.
/// </summary>
/// <remarks>An InventoryTransaction tracks adjustments to inventory levels, such as additions or removals, for a
/// particular item. The Type property indicates whether the transaction increases ("IN") or decreases ("OUT") the
/// inventory. This class is typically used to audit inventory movements and maintain accurate stock records.</remarks>
public class InventoryTransaction
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier for the item.
    /// </summary>
    /// <remarks>The ItemId property is used to uniquely identify an item within a collection or database. It
    /// is essential for operations that require item retrieval or manipulation based on its identifier.</remarks>
    public int ItemId { get; set; }
    /// <summary>
    /// Gets or sets the inventory item associated with this instance.
    /// </summary>
    /// <remarks>This property enables retrieval and assignment of the related inventory item. Use this
    /// property to access or update the details of the item managed by the inventory system.</remarks>
    public InventoryItem Item { get; set; } = new InventoryItem();
    /// <summary>
    /// Gets or sets the quantity that has been changed as part of an inventory operation.
    /// </summary>
    /// <remarks>Assigning a negative value to this property is not recommended, as negative quantities may
    /// lead to unexpected behavior in inventory calculations. Ensure that the value reflects the intended adjustment to
    /// the inventory item.</remarks>
    public int QuantityChanged { get; set; } 
    /// <summary>
    /// Gets or sets the date and time when the transaction occurred.
    /// </summary>
    /// <remarks>The value is initialized to the default value of <see cref="DateTime"/> if not explicitly
    /// set. This property should be assigned to accurately reflect the moment the transaction takes place.</remarks>
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Gets or sets the type of operation, which can be either "IN" for input or "OUT" for output.
    /// </summary>
    /// <remarks>Use this property to indicate the direction of the operation. The value should be set to
    /// either "IN" to represent an input operation or "OUT" to represent an output operation.</remarks>
    public string Type { get; set; } = "IN";// "IN" or "OUT"
}


