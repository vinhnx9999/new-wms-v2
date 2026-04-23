using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.InventoryStrategy;

/// <summary>
/// Implements a first-in, first-out (FIFO) strategy for retrieving inventory items that match specified search
/// conditions.
/// </summary>
/// <remarks>This strategy filters inventory items by category and, if provided, by supplier ID, then selects
/// items in the order they were received to fulfill the requested order quantity. It adjusts the quantities of the
/// retrieved items as needed to match the order. If the requested quantity is less than or equal to zero, no items are
/// returned. This class is typically used in warehouse management scenarios where inventory rotation based on receipt
/// date is required.</remarks>
public class FifoRetrievalStrategy : IInventoryRetrievalStrategy
{
    /// <summary>
    /// Retrieves a list of inventory items that match the specified search criteria and adjusts their quantities based
    /// on the requested order quantity.
    /// </summary>
    /// <remarks>The method filters inventory items by category and, if specified, by supplier ID. Items are
    /// ordered by received date, and their quantities are adjusted to fulfill the requested order quantity. If the
    /// total available quantity is insufficient, all matching items are returned with their original
    /// quantities.</remarks>
    /// <param name="inventory">The collection of inventory items to search. Cannot be null.</param>
    /// <param name="condition">The search criteria used to filter inventory items, including category, optional supplier ID, and the desired
    /// order quantity. Cannot be null.</param>
    /// <returns>A list of inventory items that satisfy the search criteria, with their quantities adjusted to fulfill the
    /// specified order quantity. Returns an empty list if the order quantity is less than or equal to zero or if no
    /// items match the criteria.</returns>
    public List<InventoryItem> Retrieve(List<InventoryItem> inventory, SearchCondition condition)
    {
        var query = inventory.Where(i => i.Category == condition.Category);
        if(condition.SupplierId.GetValueOrDefault() > 0)
        {
            query = query.Where(i => i.SupplierId == condition.SupplierId);
        }
                
        var data = query.OrderBy(i => i.ReceivedDate).ToList();

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
