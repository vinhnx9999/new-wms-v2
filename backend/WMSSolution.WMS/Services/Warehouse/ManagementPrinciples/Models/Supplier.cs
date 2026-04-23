namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

/// <summary>
/// Represents a supplier that provides inventory items, including identification and contact information.
/// </summary>
/// <remarks>Each supplier can be associated with multiple inventory items. Use this class to manage supplier
/// details and their related inventory within the system.</remarks>
public class Supplier
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// Gets or sets the contact information for the individual or entity.
    /// </summary>
    /// <remarks>This property holds the contact details, which can include email addresses, phone numbers, or
    /// other relevant information. Ensure that the value is formatted correctly to meet the expected standards for
    /// contact information.</remarks>
    public string ContactInfo { get; set; } = "";
    /// <summary>
    /// Gets or sets the collection of inventory items associated with this entity.
    /// </summary>
    /// <remarks>The collection can be used to add, remove, or enumerate inventory items related to the
    /// entity. Ensure that the collection is properly initialized before accessing or modifying its contents.</remarks>
    public ICollection<InventoryItem> Items { get; set; } = [];
}


