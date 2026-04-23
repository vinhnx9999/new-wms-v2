using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples;

/// <summary>
/// Order Picking
/// </summary>
public class WarehouseOrderPicking(List<InventoryLocation> locations,
    List<SkuDetail> skuDetails, List<InventoryRuleSettings> ruleSettings)
{
    private readonly List<InventoryRuleSettings> _ruleSettings = ruleSettings;
    private readonly List<InventoryLocation> _locations = locations;
    private readonly List<SkuDetail> _skuDetails = skuDetails;

    /// <summary>
    /// Prepare Goods
    /// </summary>
    /// <param name="outboundOrder"></param>
    public async Task PrepareGoodsAsync(OutboundOrder outboundOrder)
    {
        // Implement order picking logic here
        // This is a placeholder for the actual implementation
        //Step 1: Analyze the outbound order and determine the required SKUs and quantities
        var required = outboundOrder.Details
            .GroupBy(d => d.SkuId)
            .Select(g => new
            {
                SkuId = g.Key,
                TotalQuantity = g.Sum(x => x.Quantity),
                SkuDetail = _skuDetails.FirstOrDefault(s => s.Id == g.Key)
            })
            .ToList();
        //Step 2: Apply inventory rules to determine the best locations for picking
        var pickingPlan = new List<(int SkuId, int Quantity, InventoryLocation Location)>();
        foreach (var item in required)
        {
            int remainingQuantity = item.TotalQuantity;
            var locationsWithStock = CollectingInventoryByRules(item.SkuId, _locations);            
            foreach (var location in locationsWithStock)
            {
                if (remainingQuantity <= 0)
                    break;

                int availableQuantity = location.AvailableQuantity;
                int quantityToPick = Math.Min(availableQuantity, remainingQuantity);
                pickingPlan.Add((item.SkuId, quantityToPick, location.Location));
                remainingQuantity -= quantityToPick;
            }

            if (remainingQuantity > 0)
            {
                // Handle backorder or insufficient stock scenario
                Console.WriteLine($"Warning: Not enough stock for SKU {item.SkuId}. Remaining quantity: {remainingQuantity}");
            }
        }
        //Step 3: Generate picking instructions for warehouse

        //Step 4: Update inventory records to reflect the picking process

        //Step 5: Handle any exceptions or special cases, such as backorders or substitutions

        //Step 6: Ensure that the picked goods are ready for shipment by the delivery date

        //Step 7: Continuously optimize the picking process for efficiency and accuracy

        //Step 8: Provide planning on the picking process for future improvements

        // Note: The actual implementation would depend on the specific business rules, warehouse layout, and technology used in the warehouse management system.
    }

    private IEnumerable<InventoryAvailableQty> CollectingInventoryByRules(int skuId, 
        IEnumerable<InventoryLocation> locations)
    {
        //int availableQuantity = location.Pallet != null && location.Pallet.SkuId == item.SkuId
        //            ? location.Pallet.Quantity
        //            : 0;

        var rules = _ruleSettings
            .Where(r => r.Details.Any(x => x.SkuId == skuId))
            .ToList();
        return [];
    }
}

/// <summary>
/// Inventory Available Qty
/// </summary>
public class InventoryAvailableQty
{
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; } = 0;
    /// <summary>
    /// Location
    /// </summary>
    public InventoryLocation Location { get; set; } = new();
    /// <summary>
    /// Available Quantity
    /// </summary>
    public int AvailableQuantity { get; set; } = 0;
    /// <summary>
    /// Quantity To Pick
    /// </summary>
    public int QuantityToPick { get; set; } = 0;
}

/// <summary>
/// Outbound Order
/// </summary>
public class OutboundOrder
{
    /// <summary>
    /// Order Id
    /// </summary>
    public string OrderId { get; set; } = "";
    /// <summary>
    /// Delivery Date
    /// </summary>
    public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Details
    /// </summary>
    public List<OutboundOrderDetail> Details { get; set; } = [];
}

/// <summary>
/// Outbound Order
/// </summary>
public class OutboundOrderDetail
{
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; } = 0;
    /// <summary>
    /// Request quantity
    /// </summary>
    public int Quantity { get; set; } = 0;
}