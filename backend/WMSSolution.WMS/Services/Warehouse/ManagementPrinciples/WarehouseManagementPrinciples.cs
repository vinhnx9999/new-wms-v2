using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.InventoryStrategy;
using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples;

/// <summary>
/// Warehouse Management Principles
/// </summary>
public class WarehouseManagementPrinciples(List<InventoryItem> inventory, 
    IInventoryRetrievalStrategy strategy)
{
    /// <summary>
    /// Gets or sets the collection of inventory items managed by the inventory system.
    /// </summary>
    /// <remarks>Modifications to the inventory should be performed through the appropriate methods to ensure
    /// data integrity. Direct manipulation of the collection may lead to inconsistent state.</remarks>
    private readonly List<InventoryItem> _inventory = inventory;
    private readonly List<InventoryTransaction> _transactions = [];

    private IInventoryRetrievalStrategy _retrievalStrategy = strategy;

    /// <summary>
    /// Sets the inventory retrieval strategy used for inventory operations.
    /// </summary>
    /// <remarks>Changing the retrieval strategy may affect how inventory items are accessed and managed.
    /// Ensure that the provided strategy is compatible with the intended inventory operations.</remarks>
    /// <param name="strategy">The strategy to use for retrieving inventory items. This parameter cannot be null.</param>
    public void SetRetrievalStrategy(IInventoryRetrievalStrategy strategy)
    {
        _retrievalStrategy = strategy;
    }

    /// <summary>
    /// Get Items
    /// </summary>
    /// <param name="category"></param>
    /// <param name="qty"></param>
    /// <param name="supplierId"></param>
    /// <returns></returns>
    public List<InventoryItem> GetItems(string category, int qty, int supplierId)
    {
        SearchCondition condition = new()
        {
            Category = category,
            OrderQuantity = qty,
            SupplierId = supplierId
        };

        return _retrievalStrategy.Retrieve(_inventory, condition);
    }

    /// <summary>
    /// Retrieves a list of inventory items that match the specified category and have a quantity equal to or greater
    /// than the specified amount.
    /// </summary>
    /// <remarks>This method uses a retrieval strategy to filter items based on the provided category and
    /// quantity.</remarks>
    /// <param name="category">The category of the inventory items to retrieve. This parameter cannot be null or empty.</param>
    /// <param name="qty">The minimum quantity of items to retrieve. Must be a non-negative integer.</param>
    /// <returns>A list of <see cref="InventoryItem"/> objects that meet the specified criteria. The list will be empty if no
    /// items match the conditions.</returns>
    public List<InventoryItem> GetItemsByQuantity(string category, int qty)
    {
        SearchCondition condition = new()
        {
            Category = category,
            OrderQuantity = qty
        };

        return _retrievalStrategy.Retrieve(_inventory, condition);
    }

    /// <summary>
    /// Retrieves a list of inventory items that belong to the specified category.
    /// </summary>
    /// <remarks>Ensure that the category provided is valid and exists in the inventory to avoid retrieving an
    /// empty list.</remarks>
    /// <param name="category">The category of items to retrieve. This parameter cannot be null or empty.</param>
    /// <returns>A list of <see cref="InventoryItem"/> objects that match the specified category. The list will be empty if no
    /// items are found.</returns>
    public List<InventoryItem> GetItemsByCategory(string category)
    {
        SearchCondition condition = new()
        {
            Category = category
        };

        return _retrievalStrategy.Retrieve(_inventory, condition);
    }

    /// <summary>
    /// Add Item
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(InventoryItem item)
    {
        _inventory.Add(item);
        LogTransaction(item.Id, item.Quantity, "IN");
    }
    /// <summary>
    /// Adds a collection of inventory items to the inventory.
    /// </summary>
    /// <remarks>This method modifies the current inventory by appending the specified items. Ensure that the
    /// items being added are valid and do not duplicate existing entries.</remarks>
    /// <param name="items">A list of <see cref="InventoryItem"/> objects to be added to the inventory. The list cannot be null.</param>
    public void AddItems(List<InventoryItem> items)
    {
        _inventory.AddRange(items);
        foreach (var item in items)
        {
            LogTransaction(item.Id, item.Quantity, "IN");
        }
    }

    /// <summary>
    /// Remove Item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="quantity"></param>
    public void RemoveItem(int id, int quantity)
    {
        var item = _inventory.FirstOrDefault(x => x.Id == id);
        if (item != null && item.Quantity >= quantity)
        {
            item.Quantity -= quantity;
            LogTransaction(id, -quantity, "OUT");
        }
    }

    /// <summary>
    /// Items To Reorder
    /// </summary>
    /// <returns></returns>
    public List<InventoryItem> GetItemsToReorder() => [.. _inventory.Where(i => i.Quantity <= i.ReorderLevel)];

    /// <summary>
    /// Retrieves the first inventory item in the specified category based on the received date, following a first-in,
    /// first-out (FIFO) order.
    /// </summary>
    /// <remarks>This method orders the items by their received date to ensure FIFO retrieval. If multiple
    /// items share the same received date, the first one encountered will be returned.</remarks>
    /// <param name="category">The category of the inventory items to filter by. This parameter cannot be null or empty.</param>
    /// <returns>An instance of <see cref="InventoryItem"/> representing the first item in the specified category, or null if no
    /// items are found.</returns>
    public InventoryItem? GetItemFIFO(string category)
    {
        return _inventory.Where(i => i.Category == category)
                        .OrderBy(i => i.ReceivedDate)
                        .FirstOrDefault();
    }

    /// <summary>
    /// Retrieves the most recently received inventory item in the specified category using a Last-In, First-Out (LIFO)
    /// approach.
    /// </summary>
    /// <remarks>This method returns the latest item based on the <c>ReceivedDate</c> property. If multiple
    /// items share the same received date, one of them will be returned.</remarks>
    /// <param name="category">The category of inventory items to filter by. This parameter cannot be null or empty.</param>
    /// <returns>An <see cref="InventoryItem"/> representing the most recently received item in the specified category, or <see
    /// langword="null"/> if no items are found.</returns>
    public InventoryItem? GetItemLIFO(string category)
    {
        return _inventory.Where(i => i.Category == category)
                        .OrderByDescending(i => i.ReceivedDate)
                        .FirstOrDefault();
    }

    /// <summary>
    /// Retrieves the first inventory item from the specified category that is closest to its expiration date.
    /// </summary>
    /// <remarks>This method uses a first-expired, first-out (FEFO) approach to prioritize items based on
    /// their expiration dates.</remarks>
    /// <param name="category">The category of the inventory items to filter by. This parameter cannot be null or empty.</param>
    /// <returns>An instance of <see cref="InventoryItem"/> representing the first item in the specified category with the
    /// earliest expiration date, or null if no such item exists.</returns>
    public InventoryItem? GetItemFEFO(string category)
    {
        return _inventory.Where(i => i.Category == category)
                        .OrderBy(i => i.ExpirationDate)
                        .FirstOrDefault();
    }

    ///// <summary>
    ///// Retrieves a list of inventory items from the specified category, ordered by their expiration date, and limited
    ///// to the specified quantity according to the First-Expired, First-Out (FEFO) principle.
    ///// </summary>
    ///// <remarks>This method filters inventory items by the provided category and sorts them by their
    ///// expiration date to prioritize items that are closest to expiring. Ensure that the category specified is valid to
    ///// avoid unexpected results.</remarks>
    ///// <param name="category">The category of inventory items to retrieve. This parameter cannot be null or empty.</param>
    ///// <param name="qty">The maximum number of items to return. Must be a positive integer.</param>
    ///// <returns>A list of InventoryItem objects that match the specified category, ordered by expiration date. The list may
    ///// contain fewer items than requested if there are not enough available.</returns>
    //public List<InventoryItem> GetListByFEFO(string category, int qty)
    //{
    //    var data = _inventory.Where(i => i.Category == category)
    //                    .OrderBy(i => i.ExpirationDate)
    //                    .ToList();

    //    return GetItemsByQuantity(data, qty);
    //}

    private void LogTransaction(int itemId, int qty, string type)
    {
        _transactions.Add(new InventoryTransaction
        {
            ItemId = itemId,
            QuantityChanged = qty,
            TransactionDate = DateTime.UtcNow,
            Type = type
        });
    }

    //private List<InventoryItem> GetItemsByQuantity(List<InventoryItem> data, int qty)
    //{
    //    var results = new List<InventoryItem>();
    //    int quantity = 0;
    //    foreach (var item in data)
    //    {
    //        if (item.Quantity >= qty)
    //        {
    //            item.Quantity = qty;
    //            results.Add(item);
    //            return results;
    //        }

    //        if ((item.Quantity + quantity) >= qty)
    //        {
    //            item.Quantity = qty - quantity;
    //            results.Add(item);
    //            return results;
    //        }

    //        quantity += item.Quantity;
    //        results.Add(item);
    //    }

    //    return results;
    //}

    //public object SearchDataByFEFO(string category, int qty, int id)
    //{
    //    throw new NotImplementedException();
    //}
}
