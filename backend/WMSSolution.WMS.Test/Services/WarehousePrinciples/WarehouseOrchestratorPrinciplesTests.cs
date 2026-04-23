using System.Data;
using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples;
using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;
namespace WMSSolution.WMS.Test.Services.WarehousePrinciples;

[TestClass]
public class WarehouseOrchestratorPrinciplesTests : OrchestratorPrinciples
{ 
    [TestInitialize]
    public void Setup()
    {
        List<InventoryLocation> locations = InitDataLocations();
        List<InventoryRuleSettings> ruleSettings = InitDataSettings();

        _locations = locations;
        _ruleSettings = ruleSettings;
        _principles = new WarehouseOrchestratorPrinciples(locations, _skus, ruleSettings);
    }

    [TestMethod]
    public void Receiving_FIFO_InboundGoods_MergeSuppliers_ReturnList()
    {
        // Arrange - No data in database
        var inboundGoods = SampleInboundGoods();
        List<InventoryLocation> locations = InitSample_FIFO_Locations();
        List<InventoryRuleSettings> ruleSettings = InitSample_FIFO_Settings();
        var principles = new WarehouseOrchestratorPrinciples(locations, _skus, ruleSettings);
        // Act
        var results = principles.ReceivingInboundGoods(inboundGoods, true);        
        var items = GetMergeDataByFIFO(locations, inboundGoods, ruleSettings);
        var ids = string.Join("-", items.Select(x => x.Id));
        var rsIds = string.Join("-", results.Select(x => x.Id));

        // Assert
        Assert.IsNotNull(results);
        Assert.HasCount(items.Count, results);
        Assert.AreEqual(ids, rsIds);
    }

    [TestMethod]
    public void Receiving_FIFO_InboundGoods_OnlySupplier_ReturnList()
    {
        // Arrange - No data in database
        var inboundGoods = SampleInboundGoods();
        List<InventoryLocation> locations = InitSample_FIFO_Locations();
        List<InventoryRuleSettings> ruleSettings = InitSample_FIFO_Settings();
        var principles = new WarehouseOrchestratorPrinciples(locations, _skus, ruleSettings);
        // Act
        var results = principles.ReceivingInboundGoods(inboundGoods);        
        var items = GetDataByFIFO(locations, inboundGoods, ruleSettings);
        var ids = string.Join("-", items.Select(x => x.Id));
        var rsIds = string.Join("-", results.Select(x => x.Id));

        // Assert
        Assert.IsNotNull(results);
        Assert.HasCount(items.Count, results);
        Assert.AreEqual(ids, rsIds);
    }

    [TestMethod]
    public void Receiving_FIFO_NoPlanYet_ReturnRandomLocations()
    {
        // Arrange - No data in database
        var inboundGoods = SampleNoPlanYet();
        // Act
        var results = _principles.ReceivingInboundGoods(inboundGoods);        
        var notPlanDetails = _principles.NoPlanYet;
        var items = GetDataByFIFO(_locations, inboundGoods, _ruleSettings);
        var ids = string.Join("-", items.Select(x => x.Id));
        var rsIds = string.Join("-", results.Select(x => x.Id));

        // Assert
        Assert.IsNotNull(results);
        Assert.HasCount(items.Count, results);
        Assert.HasCount(DataNoPlanYet.Count, notPlanDetails);
        Assert.AreEqual(ids, rsIds);
    }

    [TestMethod]
    public void Receiving_MergeSuppliers_NoPlanYet_ReturnRandomLocations()
    {
        // Arrange - No data in database
        var inboundGoods = SampleNoPlanYet();
        // Act
        var results = _principles.ReceivingInboundGoods(inboundGoods, true);
        var notPlanDetails = _principles.NoPlanYet; 
        var items = GetMergeDataByFIFO(_locations, inboundGoods, _ruleSettings);
        var ids = string.Join("-", items.Select(x => x.Id));
        var rsIds = string.Join("-", results.Select(x => x.Id));

        // Assert
        Assert.IsNotNull(results);
        Assert.HasCount(items.Count, results);
        Assert.HasCount(DataNoPlanYet.Count, notPlanDetails);
        Assert.AreEqual(ids, rsIds);
    }

    [TestMethod]
    public void Receiving_InboundGoods_ReturnLocationList()
    {
        // Arrange - No data in database
        var inboundGoods = RandomInboundGoods();
        // Act

        // Randomly decide whether to merge suppliers or not
        bool isMergeSuppliers = new Random().Next(0, 2) == 0;
        var results = _principles.ReceivingInboundGoods(inboundGoods, isMergeSuppliers);
        var notPlanDetails = _principles.NoPlanYet;

        var items = isMergeSuppliers ?
            GetMergeDataByFIFO(_locations, inboundGoods, _ruleSettings):
            GetDataByFIFO(_locations, inboundGoods, _ruleSettings);

        var ids = string.Join("-", items.Select(x => x.Id));
        var rsIds = string.Join("-", results.Select(x => x.Id));

        // Assert
        Assert.IsNotNull(results);
        Assert.HasCount(items.Count, results);
        Assert.HasCount(DataNoPlanYet.Count, notPlanDetails);
        Assert.AreEqual(ids, rsIds);
    }

    #region Private methods

    private List<PlanAccuracyLocation> accuracyLocations = [];
    private List<InboundDetail> DataNoPlanYet { get; set; } = [];

    private List<PlanAccuracyLocation> GetMergeDataByFIFO(List<InventoryLocation> locations,
        InboundGoods inboundGoods, List<InventoryRuleSettings> ruleSettings)
    {
        var results = new List<PlanAccuracyLocation>();
        accuracyLocations = [];
        DataNoPlanYet = [];
        foreach (var item in inboundGoods.Details)
        {
            int maxQty = _skus.FirstOrDefault(x => x.Id == item.SkuId)?.MaxQuantityPerPallet ?? 1;
            var ids = accuracyLocations
                .Where(x => x.PlannedQuantity >= maxQty)
                .Select(x => x.Id).Distinct();

            var inventories = locations.Where(x => !ids.Contains(x.Id));
            var locationsBySupplier = ApplyRuleSettings(inventories, item, ruleSettings);

            var items = GetItemsByCondition(item, maxQty, locationsBySupplier, accuracyLocations);
            if (!items.Any())
            {
                DataNoPlanYet.Add(item);
                continue;
            }

            results.AddRange(items);
        }

        return results;
    }

    private List<PlanAccuracyLocation> GetDataByFIFO(List<InventoryLocation> locations, 
        InboundGoods inboundGoods, List<InventoryRuleSettings> ruleSettings)
    {
        var results = new List<PlanAccuracyLocation>();
        accuracyLocations = [];
        DataNoPlanYet = [];
        foreach (var item in inboundGoods.Details)
        {
            int maxQty = _skus.FirstOrDefault(x => x.Id == item.SkuId)?.MaxQuantityPerPallet ?? 1;
            var ids = accuracyLocations.Select(x => x.Id).Distinct();

            var inventories = locations.Where(x => !ids.Contains(x.Id));
            var locationsBySupplier = ApplyRuleSettings(inventories, item, ruleSettings);

            var items = GetItemsByCondition(item, maxQty, locationsBySupplier, accuracyLocations);
            if (!items.Any())
            {
                DataNoPlanYet.Add(item);
                continue;
            }

            results.AddRange(items);
        }

        return results;
    }

    private IEnumerable<InventoryLocation> ApplyRuleSettings(IEnumerable<InventoryLocation> inventories,
        InboundDetail item, List<InventoryRuleSettings> ruleSettings)
    {
        var results = new List<InventoryLocation>();
        bool applyForSupplier = false;

        foreach (var setting in ruleSettings)
        {
            bool hasSupplier = !setting.SupplierIds.Any() || setting.SupplierIds.Contains(item.SupplierId);
            if (hasSupplier)
            {
                applyForSupplier = true;
                var details = setting.Details.Where(x => x.SkuId == item.SkuId);

                foreach (var detail in details)
                {
                    var items = inventories.Where(x => !x.PalletId.HasValue || !x.Pallet.IsFull)
                        .Where(x => x.BlockId == detail.BlockId && x.FloorId == detail.FloorId);

                    results.AddRange(items);
                }
            }
        }

        if (!applyForSupplier || results.Count == 0)
        {
            return inventories.Where(x => !x.PalletId.HasValue || !x.Pallet.IsFull);
        }

        return results;
    }

    private IEnumerable<PlanAccuracyLocation> GetItemsByCondition(InboundDetail item, int maxQty,
        IEnumerable<InventoryLocation> inventories, List<PlanAccuracyLocation> accuracyLocations)
    {
        var results = new List<PlanAccuracyLocation>();        
        int qty = item.Quantity;
        
        foreach (var location in inventories.OrderBy(x => x.FloorId).ThenBy(x => x.Priority))
        {
            var existingPlannedQty = accuracyLocations
                .Where(x => x.Id == location.Id)
                .Sum(x => x.PlannedQuantity);

            if (existingPlannedQty >= maxQty) continue;

            int availableQty = maxQty - existingPlannedQty;
            if (availableQty >= qty)
            {
                var newItem = new PlanAccuracyLocation(location, OperationRules.FIFO)
                {
                    PlannedQuantity = qty,
                    SkuId = item.SkuId,
                    SupplierId = item.SupplierId
                };

                results.Add(newItem);
                if (qty > 0) accuracyLocations.Add(newItem);
                return [.. results];
            }
            else if (availableQty > 0)
            {
                var newItem = new PlanAccuracyLocation(location, OperationRules.FIFO)
                {
                    PlannedQuantity = availableQty,
                    SkuId = item.SkuId,
                    SupplierId = item.SupplierId
                };

                results.Add(newItem);
                accuracyLocations.Add(newItem);
                qty -= availableQty;
            }
        }

        return results;
    }

    #endregion

}
