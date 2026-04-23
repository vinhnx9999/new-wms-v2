using System.Data;
using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples;

/// <summary>
/// Warehouse Orchestrator Principles
/// </summary>
/// <param name="locations"></param>
/// <param name="skuDetails"></param>
/// <param name="ruleSettings"></param>
public class WarehouseOrchestratorPrinciples(List<InventoryLocation> locations,
    List<SkuDetail> skuDetails, List<InventoryRuleSettings> ruleSettings)
{
    private readonly List<InventoryRuleSettings> _ruleSettings = ruleSettings;
    private readonly List<InventoryLocation> _locations = locations;
    private readonly List<SkuDetail> _skuDetails = skuDetails;
    private List<InboundDetail> Details { get; set; } = [];
    private List<PlanAccuracyLocation> accuracyLocations = [];

    /// <summary>
    /// No plan details
    /// </summary>
    public List<InboundDetail> NoPlanYet => Details;
    
    /// <summary>
    /// Receiving Inbound Goods
    /// </summary>
    /// <param name="inbound"></param>
    /// <param name="mergeSupplier"></param>
    public List<PlanAccuracyLocation> ReceivingInboundGoods(
        InboundGoods inbound, bool mergeSupplier = false)
    {
        var results = new List<PlanAccuracyLocation>();
        accuracyLocations = [];
        Details = [];
        foreach (var item in inbound.Details)
        {
            int maxQtyPerPallet = _skuDetails.Where(x => x.Id == item.SkuId)
                .FirstOrDefault()?
                .MaxQuantityPerPallet ?? 1;

            var excludeLocations = accuracyLocations
                .Where(x => x.PlannedQuantity >= maxQtyPerPallet)
                .Select(x => x.Id);

            if (!mergeSupplier)
            {
                excludeLocations = accuracyLocations.Select(x => x.Id);
            }

            var inventoryLocations = GetLocationsBySupplier(item, excludeLocations);
            if (inventoryLocations.Count < 1)
            {
                Details.Add(item);
                continue;
            }

            var planLocations = PlanReceivingLocations(inventoryLocations,
                item.SkuId, item.Quantity, maxQtyPerPallet);
            if (!planLocations.Any())
            {
                Details.Add(item);
                continue;
            }

            results.AddRange(planLocations);
        }

        return results;
    }

    private IEnumerable<PlanAccuracyLocation> PlanReceivingLocations(
        IEnumerable<PlanAccuracyLocation> locations,
        int skuId, int qty, int maxQtyPerPallet)
    {
        var results = new List<PlanAccuracyLocation>();
        var items = locations.Distinct().OrderBy(x => x.FloorId).ThenBy(x => x.Priority);
        foreach (var item in items)
        {
            var existingPlannedQty = accuracyLocations
                .Where(x => x.Id == item.Id)
                .Sum(x => x.PlannedQuantity);

            if (existingPlannedQty >= maxQtyPerPallet) continue;

            int availableQty = maxQtyPerPallet - existingPlannedQty;
            if (item.PalletId != null)
            {
                int currentQuantity = item.Pallet.Details
                    .Where(x => x.SkuId == skuId)
                    .Sum(x => x.Quantity);

                availableQty = currentQuantity > maxQtyPerPallet ? 0 
                    : maxQtyPerPallet - currentQuantity;
            }

            if (availableQty >= qty)
            {
                item.PlannedQuantity = qty;
                results.Add(item);
                if (qty > 0) accuracyLocations.Add(item);
                return [.. results];
            }
            else if (availableQty > 0)
            {
                item.PlannedQuantity = availableQty;
                accuracyLocations.Add(item);
                results.Add(item);
                qty -= availableQty;
            }
        }

        return [.. results];
    }

    private List<PlanAccuracyLocation> GetLocationsBySupplier(
        InboundDetail item, IEnumerable<int> excludeLocations)
    {
        var supplierId = item.SupplierId;
        var skuId = item.SkuId;
        var supplierLocations = new List<PlanAccuracyLocation>();

        bool applyForSupplier = false;
        var inventories = _locations.Where(x => !excludeLocations.Contains(x.Id));

        foreach (var rule in _ruleSettings)
        {
            bool hasSupplier = !rule.SupplierIds.Any() || rule.SupplierIds.Contains(supplierId);
            if (hasSupplier)
            {
                applyForSupplier = true;
                var details = rule.Details.Where(x => x.SkuId == skuId);

                var locations = GetInventoryLocationByRules(inventories, details, 
                    rule.RuleSettings, skuId, supplierId);
                supplierLocations.AddRange(locations);
            }            
        }

        //Not yet setting rule for Supplier, we will get all locations
        if (!applyForSupplier || supplierLocations.Count == 0)
        {
            var locations = GetInventoryLocationByDefault(inventories, skuId, supplierId);
            supplierLocations.AddRange(locations);
        }
        
        return [.. supplierLocations];
    }

    private IEnumerable<PlanAccuracyLocation> GetInventoryLocationByDefault(
        IEnumerable<InventoryLocation> inventories, int skuId, int supplierId)
    {
        var defaultLocations = new List<PlanAccuracyLocation>();
        var items = inventories.Where(x => !x.PalletId.HasValue || !x.Pallet.IsFull);

        foreach (var item in items)
        {
            defaultLocations.Add(new PlanAccuracyLocation(item, OperationRules.Custom)
            {
                SkuId = skuId,
                SupplierId = supplierId
            });
        }

        return [.. defaultLocations];
    }

    private IEnumerable<PlanAccuracyLocation> GetInventoryLocationByRules(
        IEnumerable<InventoryLocation> inventories, IEnumerable<RuleDetail> details, 
        OperationRules ruleSettings, int skuId, int supplierId)
    {
        var locationByRules = new List<PlanAccuracyLocation>();
        foreach (var rule in details) 
        {
            var items = inventories.Where(x => !x.PalletId.HasValue || !x.Pallet.IsFull)
                .Where(x => x.BlockId == rule.BlockId && x.FloorId == rule.FloorId);

            foreach (var item in items)
            {
                locationByRules.Add(new PlanAccuracyLocation (item, ruleSettings)
                {
                    SkuId = skuId,
                    SupplierId = supplierId
                });
            }
        }

        return [.. locationByRules];
    }
}
