using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.InventoryStrategy;

/// <summary>
/// Defines a strategy for retrieving inventory items that match the specified category and quantity criteria.
/// </summary>
/// <remarks>Implementations of this interface should provide the logic for filtering inventory items based on the
/// given category and quantity. This allows for flexible retrieval strategies, such as prioritizing certain categories
/// or handling quantity thresholds differently. The interface enables extensibility for various inventory management
/// scenarios.</remarks>
public interface IInventoryRetrievalStrategy
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    List<InventoryItem> Retrieve(List<InventoryItem> inventory, SearchCondition condition);
}
