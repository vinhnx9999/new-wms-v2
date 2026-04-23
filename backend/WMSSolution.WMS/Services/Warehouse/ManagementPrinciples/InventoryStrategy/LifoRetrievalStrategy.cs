using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.InventoryStrategy;

/// <summary>
/// Implements a last-in, first-out (LIFO) inventory retrieval strategy that selects items based on specified search
/// conditions.
/// </summary>
/// <remarks>This strategy retrieves inventory items that match the provided category and, if specified, supplier
/// ID. Items are prioritized by their most recent received date, ensuring that the newest inventory is selected first.
/// When an order quantity is specified, the strategy attempts to fulfill the requested quantity using available items,
/// returning as many items as needed to meet the order. This class is typically used in warehouse management scenarios
/// where LIFO inventory handling is required.</remarks>
public class LifoRetrievalStrategy : IInventoryRetrievalStrategy
{
    /// <summary>
    /// Retrieves a list of inventory items that match the specified search criteria, optionally filtered by supplier
    /// and limited by the requested order quantity.
    /// </summary>
    /// <remarks>Items are prioritized by most recent received date. If the requested order quantity cannot be
    /// fulfilled by a single item, quantities from multiple items are combined in the result as needed.</remarks>
    /// <param name="inventory">The collection of inventory items to search. Each item is evaluated against the provided search condition.</param>
    /// <param name="condition">The search criteria used to filter inventory items, including category, optional supplier identifier, and the
    /// desired order quantity.</param>
    /// <returns>A list of inventory items that satisfy the search condition. If an order quantity is specified and greater than
    /// zero, the returned items' quantities are adjusted to fulfill the requested amount. If the order quantity is less
    /// than or equal to zero, all matching items are returned.</returns>
    public List<InventoryItem> Retrieve(List<InventoryItem> inventory, SearchCondition condition)
    {
        var query = inventory.Where(i => i.Category == condition.Category);
        if (condition.SupplierId.GetValueOrDefault() > 0)
        {
            query = query.Where(i => i.SupplierId == condition.SupplierId);
        }

        var data = query.OrderByDescending(i => i.ReceivedDate).ToList();

        int qty = condition.OrderQuantity;
        if (qty <= 0)
        {
            return data;
        }

        int quantity = 0;
        var results = new List<InventoryItem>();
        foreach (var item in data)
        {
            if (item.Quantity >= qty)
            {
                item.Quantity = qty;
                results.Add(item);
                return results;
            }

            if ((item.Quantity + quantity) >= qty)
            {
                item.Quantity = qty - quantity;
                results.Add(item);
                return results;
            }

            quantity += item.Quantity;
            results.Add(item);
        }

        return results;
    }
}
