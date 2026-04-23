using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels.Goodslocation;
using WMSSolution.WMS.Services.Goodslocation;

namespace WMSSolution.WMS.Test.Services.Goodslocation;

[TestClass]
public class GoodslocationServiceTests
{
    private SqlDBContext _dbContext = null!;
    private Mock<IStringLocalizer<MultiLanguage>> _localizerMock = null!;
    private Mock<ILogger<GoodslocationService>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<SqlDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            // InMemory provider does not support transactions; ignore warning to allow code paths using BeginTransactionAsync()
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new SqlDBContext(options);

        _localizerMock = new Mock<IStringLocalizer<MultiLanguage>>();
        _localizerMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _loggerMock = new Mock<ILogger<GoodslocationService>>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task GetLocationForPallet_WhenTotalPalletNeedLessOrEqualZero_ReturnsEmpty()
    {
        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };

        var res0 = await sut.GetLocationForPallet(user, GetLocationPalletTypeEnum.Inbound, 0);
        var resNeg = await sut.GetLocationForPallet(user, GetLocationPalletTypeEnum.Inbound, -1);

        Assert.AreEqual(0, res0.Count);
        Assert.AreEqual(0, resNeg.Count);
    }

    [TestMethod]
    public async Task GetLocationForPallet_Inbound_WhenFirstPriorityHasEnough_ReturnsExactlyNeedFromPriority1()
    {
        SeedLocations(tenantId: 1, priority: 1, ids: [101, 102, 103, 104, 105]);
        SeedLocations(tenantId: 1, priority: 2, ids: [201, 202, 203, 204, 205]);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };

        var res = await sut.GetLocationForPallet(user, GetLocationPalletTypeEnum.Inbound, totalPalletNeed: 3);

        Assert.AreEqual(3, res.Count);
        CollectionAssert.AreEqual(new[] { 101, 102, 103 }, res.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public async Task GetLocationForPallet_Inbound_WhenFirstPriorityNotEnough_ReturnsAllUpToBoundaryPriority()
    {
        // priority 1: 3 locations, priority 2: 6 locations, need 4 => boundary priority should be 2 => return 3 + 6 = 9
        SeedLocations(tenantId: 1, priority: 1, ids: [101, 102, 103]);
        SeedLocations(tenantId: 1, priority: 2, ids: [201, 202, 203, 204, 205, 206]);
        SeedLocations(tenantId: 1, priority: 3, ids: [301, 302, 303, 304, 305]); // should NOT be included

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };

        var res = await sut.GetLocationForPallet(user, GetLocationPalletTypeEnum.Inbound, totalPalletNeed: 4);

        Assert.AreEqual(9, res.Count);
        CollectionAssert.AreEqual(
            new[] { 101, 102, 103, 201, 202, 203, 204, 205, 206 },
            res.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public async Task GetLocationForPallet_Inbound_WhenNeedsThirdPriority_ReturnsAllUpToThirdPriority()
    {
        // priority 1: 1, priority 2: 1, priority 3: 10, need 3 => boundary priority should be 3 => return all <= 3 (12)
        SeedLocations(tenantId: 1, priority: 1, ids: [101]);
        SeedLocations(tenantId: 1, priority: 2, ids: [201]);
        SeedLocations(tenantId: 1, priority: 3, ids: [301, 302, 303, 304, 305, 306, 307, 308, 309, 310]);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };

        var res = await sut.GetLocationForPallet(user, GetLocationPalletTypeEnum.Inbound, totalPalletNeed: 3);

        Assert.AreEqual(12, res.Count);
        CollectionAssert.AreEqual(
            new[] { 101, 201, 301, 302, 303, 304, 305, 306, 307, 308, 309, 310 },
            res.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public async Task GetLocationForPallet_Outbound_WhenFirstPriorityHasEnough_ReturnsExactlyNeedFromHighestPriority()
    {
        // Outbound: bigger priority means higher priority.
        SeedLocations(tenantId: 1, priority: 6, ids: [601, 602, 603]);
        SeedLocations(tenantId: 1, priority: 5, ids: [501, 502, 503]);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };

        var res = await sut.GetLocationForPallet(user, GetLocationPalletTypeEnum.Outbound, totalPalletNeed: 2);

        Assert.AreEqual(2, res.Count);
        CollectionAssert.AreEqual(new[] { 601, 602 }, res.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public async Task GetLocationForPallet_Outbound_WhenNeedsNextPriority_ReturnsAllFromHighestDownToBoundary()
    {
        // priority 6: 1, priority 5: 2, need 3 => boundary priority should be 5 => return all >= 5 => 1 + 2 = 3
        SeedLocations(tenantId: 1, priority: 6, ids: [601]);
        SeedLocations(tenantId: 1, priority: 5, ids: [501, 502]);
        SeedLocations(tenantId: 1, priority: 4, ids: [401, 402, 403, 404]); // should NOT be included

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };

        var res = await sut.GetLocationForPallet(user, GetLocationPalletTypeEnum.Outbound, totalPalletNeed: 3);

        Assert.AreEqual(3, res.Count);
        // Order: priority desc then id => 6-group first, then 5-group
        CollectionAssert.AreEqual(new[] { 601, 501, 502 }, res.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public async Task GetLocationForPallet_FiltersOutInvalidOccupiedAndNonStorageSlot()
    {
        // Only this one should pass baseQuery filters
        SeedLocations(tenantId: 1, priority: 1, ids: [101], isValid: true, status: (byte)GoodLocationStatusEnum.AVAILABLE, type: GoodsLocationTypeEnum.StorageSlot);

        // Excluded: IsValid=false
        SeedLocations(tenantId: 1, priority: 1, ids: [102], isValid: false, status: (byte)GoodLocationStatusEnum.AVAILABLE, type: GoodsLocationTypeEnum.StorageSlot);
        // Excluded: status != AVAILABLE
        SeedLocations(tenantId: 1, priority: 1, ids: [103], isValid: true, status: (byte)GoodLocationStatusEnum.OCCUPIED, type: GoodsLocationTypeEnum.StorageSlot);
        // Excluded: GoodsLocationType != StorageSlot
        SeedLocations(tenantId: 1, priority: 1, ids: [104], isValid: true, status: (byte)GoodLocationStatusEnum.AVAILABLE, type: GoodsLocationTypeEnum.HorizontalPath);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };

        var res = await sut.GetLocationForPallet(user, GetLocationPalletTypeEnum.Inbound, totalPalletNeed: 10);

        Assert.AreEqual(1, res.Count);
        Assert.AreEqual(101, res[0].Id);
    }

    private GoodslocationService CreateSut()
    {
        return new GoodslocationService(_dbContext, _localizerMock.Object, _loggerMock.Object);
    }

    #region GetLocationWithPalletAsync Tests

    [TestMethod]
    public async Task GetLocationWithPalletAsync_NullRequest_ReturnsEmpty()
    {
        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };

        var result = await sut.GetLocationWithPalletAsync(null!, user, CancellationToken.None);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_NoLocationsInWarehouse_ReturnsEmpty()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 1, warehouseId: 99, locationName: "L-001");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 1, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_EmptyLocation_ReturnsWithNullItems()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 1, warehouseId: 10, locationName: "LOC-EMPTY");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 10, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        var loc = result[0];
        Assert.AreEqual(1, loc.GoodLocationId);
        Assert.AreEqual("LOC-EMPTY", loc.GoodLocationName);
        Assert.AreEqual(0, loc.PalletId);
        Assert.AreEqual("", loc.PalletCode);
        Assert.IsNull(loc.Items);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_LocationWithNonFullPallet_ReturnsPalletAndItems()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 10, warehouseId: 5, locationName: "LOC-A");
        SeedPallet(tenantId: 1, id: 50, palletCode: "PAL-001", isFull: false);
        SeedSku(id: 100, skuName: "Widget-X");
        SeedStock(tenantId: 1, goodsLocationId: 10, palletCode: "PAL-001", skuId: 100, qty: 25);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        var loc = result[0];
        Assert.AreEqual(10, loc.GoodLocationId);
        Assert.AreEqual("LOC-A", loc.GoodLocationName);
        Assert.AreEqual(50, loc.PalletId);
        Assert.AreEqual("PAL-001", loc.PalletCode);
        Assert.IsNotNull(loc.Items);
        Assert.AreEqual(1, loc.Items.Count);
        Assert.AreEqual(100, loc.Items[0].SkuId);
        Assert.AreEqual("Widget-X", loc.Items[0].SkuName);
        Assert.AreEqual(25, loc.Items[0].Qty);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_MultipleSkusInPallet_ReturnsAllItems()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 10, warehouseId: 5, locationName: "LOC-MULTI");
        SeedPallet(tenantId: 1, id: 50, palletCode: "PAL-MIX", isFull: false);
        SeedSku(id: 100, skuName: "SKU-Alpha");
        SeedSku(id: 101, skuName: "SKU-Beta");
        SeedSku(id: 102, skuName: "SKU-Gamma");
        SeedStock(tenantId: 1, goodsLocationId: 10, palletCode: "PAL-MIX", skuId: 100, qty: 10);
        SeedStock(tenantId: 1, goodsLocationId: 10, palletCode: "PAL-MIX", skuId: 101, qty: 20);
        SeedStock(tenantId: 1, goodsLocationId: 10, palletCode: "PAL-MIX", skuId: 102, qty: 5);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        var items = result[0].Items;
        Assert.IsNotNull(items);
        Assert.AreEqual(3, items.Count);
        CollectionAssert.AreEquivalent(
            new[] { 100, 101, 102 },
            items.Select(i => i.SkuId).ToArray());
        Assert.AreEqual(10, items.First(i => i.SkuId == 100).Qty);
        Assert.AreEqual(20, items.First(i => i.SkuId == 101).Qty);
        Assert.AreEqual(5, items.First(i => i.SkuId == 102).Qty);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_FullPallet_IsExcluded()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-FULL");
        SeedPallet(tenantId: 1, id: 10, palletCode: "PAL-FULL", isFull: true);
        SeedStock(tenantId: 1, goodsLocationId: 1, palletCode: "PAL-FULL", skuId: 1, qty: 50);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_MixOfEmptyAndOccupied_ReturnsBoth()
    {
        // Empty location
        SeedLocationForWarehouse(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-EMPTY");

        // Location with non-full pallet
        SeedLocationForWarehouse(tenantId: 1, id: 2, warehouseId: 5, locationName: "LOC-PARTIAL");
        SeedPallet(tenantId: 1, id: 10, palletCode: "PAL-OPEN", isFull: false);
        SeedSku(id: 100, skuName: "Item-A");
        SeedStock(tenantId: 1, goodsLocationId: 2, palletCode: "PAL-OPEN", skuId: 100, qty: 5);

        // Location with full pallet → excluded
        SeedLocationForWarehouse(tenantId: 1, id: 3, warehouseId: 5, locationName: "LOC-FULL");
        SeedPallet(tenantId: 1, id: 11, palletCode: "PAL-FULL", isFull: true);
        SeedStock(tenantId: 1, goodsLocationId: 3, palletCode: "PAL-FULL", skuId: 100, qty: 99);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(2, result.Count);

        var empty = result.First(r => r.GoodLocationId == 1);
        Assert.AreEqual("LOC-EMPTY", empty.GoodLocationName);
        Assert.AreEqual(0, empty.PalletId);
        Assert.IsNull(empty.Items);

        var partial = result.First(r => r.GoodLocationId == 2);
        Assert.AreEqual("LOC-PARTIAL", partial.GoodLocationName);
        Assert.AreEqual("PAL-OPEN", partial.PalletCode);
        Assert.IsNotNull(partial.Items);
        Assert.AreEqual(1, partial.Items.Count);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_FiltersByWarehouseId()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 1, warehouseId: 5, locationName: "WH5-LOC");
        SeedLocationForWarehouse(tenantId: 1, id: 2, warehouseId: 99, locationName: "WH99-LOC");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("WH5-LOC", result[0].GoodLocationName);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_FiltersByTenant()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 1, warehouseId: 5, locationName: "T1-LOC");
        SeedLocationForWarehouse(tenantId: 2, id: 2, warehouseId: 5, locationName: "T2-LOC");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("T1-LOC", result[0].GoodLocationName);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_OnlyAvailableAndValidLocations_Returned()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 1, warehouseId: 5, locationName: "OK",
            status: (byte)GoodLocationStatusEnum.AVAILABLE, isValid: true);
        SeedLocationForWarehouse(tenantId: 1, id: 2, warehouseId: 5, locationName: "OCCUPIED",
            status: (byte)GoodLocationStatusEnum.OCCUPIED, isValid: true);
        SeedLocationForWarehouse(tenantId: 1, id: 3, warehouseId: 5, locationName: "INVALID",
            status: (byte)GoodLocationStatusEnum.AVAILABLE, isValid: false);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("OK", result[0].GoodLocationName);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_QtyLimitsResults()
    {
        for (int i = 1; i <= 5; i++)
        {
            SeedLocationForWarehouse(tenantId: 1, id: i, warehouseId: 5, locationName: $"LOC-{i}");
        }

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 3 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_QtyZeroOrExceedsCount_ReturnsAll()
    {
        for (int i = 1; i <= 3; i++)
        {
            SeedLocationForWarehouse(tenantId: 1, id: i, warehouseId: 5, locationName: $"LOC-{i}");
        }

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };

        var request0 = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 0 };
        var result0 = await sut.GetLocationWithPalletAsync(request0, user, CancellationToken.None);
        Assert.AreEqual(3, result0.Count);

        var requestBig = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 100 };
        var resultBig = await sut.GetLocationWithPalletAsync(requestBig, user, CancellationToken.None);
        Assert.AreEqual(3, resultBig.Count);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_OrdersByPriorityThenFloor()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 1, warehouseId: 5, locationName: "P2-Z2", priority: 2, coordZ: "2");
        SeedLocationForWarehouse(tenantId: 1, id: 2, warehouseId: 5, locationName: "P1-Z2", priority: 1, coordZ: "2");
        SeedLocationForWarehouse(tenantId: 1, id: 3, warehouseId: 5, locationName: "P1-Z1", priority: 1, coordZ: "1");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 10 };

        var result = await sut.GetLocationWithPalletAsync(request, user, CancellationToken.None);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("P1-Z1", result[0].GoodLocationName);
        Assert.AreEqual("P1-Z2", result[1].GoodLocationName);
        Assert.AreEqual("P2-Z2", result[2].GoodLocationName);
    }

    [TestMethod]
    public async Task GetLocationWithPalletAsync_CancellationRequested_ThrowsOperationCanceled()
    {
        SeedLocationForWarehouse(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-1");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationWithPalletRequest { WarehouseId = 5, Qty = 10 };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => sut.GetLocationWithPalletAsync(request, user, cts.Token));
    }

    #endregion

    #region Seed Helpers

    private void SeedLocations(
        long tenantId,
        int priority,
        IEnumerable<int> ids,
        bool isValid = true,
        byte status = (byte)GoodLocationStatusEnum.AVAILABLE,
        GoodsLocationTypeEnum type = GoodsLocationTypeEnum.StorageSlot)
    {
        var set = _dbContext.GetDbSet<GoodslocationEntity>();
        foreach (var id in ids)
        {
            set.Add(new GoodslocationEntity
            {
                Id = id,
                TenantId = tenantId,
                Priority = priority,
                IsValid = isValid,
                LocationStatus = status,
                GoodsLocationType = type,
                LocationName = $"L-{id}",
                WarehouseName = "W",
                WarehouseAreaName = "A"
            });
        }
        _dbContext.SaveChanges();
    }

    private void SeedLocationForWarehouse(
        long tenantId,
        int id,
        int warehouseId,
        string locationName,
        byte status = (byte)GoodLocationStatusEnum.AVAILABLE,
        bool isValid = true,
        int priority = 1,
        string coordZ = "1")
    {
        _dbContext.GetDbSet<GoodslocationEntity>().Add(new GoodslocationEntity
        {
            Id = id,
            TenantId = tenantId,
            WarehouseId = warehouseId,
            LocationName = locationName,
            LocationStatus = status,
            IsValid = isValid,
            Priority = priority,
            CoordinateZ = coordZ,
            WarehouseName = "W",
            WarehouseAreaName = "A"
        });
        _dbContext.SaveChanges();
    }

    private void SeedPallet(long tenantId, int id, string palletCode, bool isFull = false)
    {
        _dbContext.GetDbSet<PalletEntity>().Add(new PalletEntity
        {
            Id = id,
            TenantId = tenantId,
            PalletCode = palletCode,
            IsFull = isFull
        });
        _dbContext.SaveChanges();
    }

    private void SeedSku(int id, string skuName)
    {
        _dbContext.GetDbSet<SkuEntity>().Add(new SkuEntity
        {
            Id = id,
            sku_name = skuName,
            sku_code = $"CODE-{id}",
            bar_code = "",
            unit = "pcs"
        });
        _dbContext.SaveChanges();
    }

    private void SeedStock(long tenantId, int goodsLocationId, string palletCode, int skuId = 0, int qty = 1)
    {
        _dbContext.GetDbSet<StockEntity>().Add(new StockEntity
        {
            TenantId = tenantId,
            goods_location_id = goodsLocationId,
            Palletcode = palletCode,
            sku_id = skuId,
            qty = qty
        });
        _dbContext.SaveChanges();
    }

    #endregion
}

