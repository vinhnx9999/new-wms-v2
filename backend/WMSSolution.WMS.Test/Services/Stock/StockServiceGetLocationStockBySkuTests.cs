using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels.Stock;
using WMSSolution.WMS.Services.Stock;

namespace WMSSolution.WMS.Test.Services.Stock;

[TestClass]
public class StockServiceGetLocationStockBySkuTests
{
    private SqlDBContext _dbContext = null!;
    private Mock<IStringLocalizer<MultiLanguage>> _localizerMock = null!;
    private Mock<ILogger<StockService>> _loggerMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<SqlDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new SqlDBContext(options);

        _localizerMock = new Mock<IStringLocalizer<MultiLanguage>>();
        _localizerMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _loggerMock = new Mock<ILogger<StockService>>();
        _configurationMock = new Mock<IConfiguration>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }

    #region GetLocationStockBySkuAsync Tests

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_NoLocationsInWarehouse_ReturnsEmpty()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 99, locationName: "LOC-OTHER");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_LocationsExistButNoStock_ReturnsEmpty()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_StockExistsButSkuNotFound_ReturnsEmpty()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 10);
        // No SKU entity seeded for id 100

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_SingleStockRecord_ReturnsCorrectQty()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 50);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].GoodLocationId);
        Assert.AreEqual("LOC-A", result[0].GoodLocationName);
        Assert.IsNotNull(result[0].Items);
        Assert.AreEqual(1, result[0].Items!.Count);
        Assert.AreEqual(100, result[0].Items![0].SkuId);
        Assert.AreEqual("Widget", result[0].Items![0].SkuName);
        Assert.AreEqual(50, result[0].Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_FrozenStock_SubtractedFromQty()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 50, isFreeze: false);
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 20, isFreeze: true);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        // totalQty = 50 + 20 = 70, frozenQty = 20, lockedQty = 0 => available = 50
        Assert.AreEqual(50, result[0].Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_DispatchLocked_SubtractedFromQty()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 100);

        // Dispatch with status 3 (between 1 and 6 exclusive) locks pick_qty
        SeedDispatchWithPick(tenantId: 1, dispatchId: 1, dispatchStatus: 3,
            pickId: 1, skuId: 100, goodsLocationId: 1, pickQty: 30);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        // totalQty = 100, frozenQty = 0, lockedQty = 30 => available = 70
        Assert.AreEqual(70, result[0].Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_DispatchStatusOutOfRange_NotLocked()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 100);

        // Status 1 is NOT > 1, so should not lock
        SeedDispatchWithPick(tenantId: 1, dispatchId: 1, dispatchStatus: 1,
            pickId: 1, skuId: 100, goodsLocationId: 1, pickQty: 30);

        // Status 6 is NOT < 6, so should not lock
        SeedDispatchWithPick(tenantId: 1, dispatchId: 2, dispatchStatus: 6,
            pickId: 2, skuId: 100, goodsLocationId: 1, pickQty: 20);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        // No locks applied, available = 100
        Assert.AreEqual(100, result[0].Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_FrozenAndLocked_BothSubtracted()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 80, isFreeze: false);
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 20, isFreeze: true);

        SeedDispatchWithPick(tenantId: 1, dispatchId: 1, dispatchStatus: 3,
            pickId: 1, skuId: 100, goodsLocationId: 1, pickQty: 15);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        // totalQty = 100, frozenQty = 20, lockedQty = 15 => available = 65
        Assert.AreEqual(65, result[0].Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_MultipleLocations_ReturnsAll()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedLocation(tenantId: 1, id: 2, warehouseId: 5, locationName: "LOC-B");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 30);
        SeedStock(tenantId: 1, goodsLocationId: 2, skuId: 100, qty: 50);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(2, result.Count);
        var locA = result.First(r => r.GoodLocationName == "LOC-A");
        var locB = result.First(r => r.GoodLocationName == "LOC-B");
        Assert.AreEqual(30, locA.Items![0].Qty);
        Assert.AreEqual(50, locB.Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_DifferentPalletsSameLocation_ReturnsSeparateEntries()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 20, palletCode: "PAL-001");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 30, palletCode: "PAL-002");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(2, result.Count);
        var pal1 = result.First(r => r.PalletCode == "PAL-001");
        var pal2 = result.First(r => r.PalletCode == "PAL-002");
        Assert.AreEqual(20, pal1.Items![0].Qty);
        Assert.AreEqual(30, pal2.Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_InvalidLocations_Excluded()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "VALID", isValid: true);
        SeedLocation(tenantId: 1, id: 2, warehouseId: 5, locationName: "INVALID", isValid: false);
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 10);
        SeedStock(tenantId: 1, goodsLocationId: 2, skuId: 100, qty: 10);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("VALID", result[0].GoodLocationName);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_StockForDifferentSku_NotIncluded()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget-A");
        SeedSku(id: 200, skuName: "Widget-B");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 10);
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 200, qty: 99);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(10, result[0].Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_OrderedByLocationNameThenPalletCode()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-C");
        SeedLocation(tenantId: 1, id: 2, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 10, palletCode: "PAL-B");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 10, palletCode: "PAL-A");
        SeedStock(tenantId: 1, goodsLocationId: 2, skuId: 100, qty: 10, palletCode: "PAL-Z");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("LOC-A", result[0].GoodLocationName);
        Assert.AreEqual("LOC-C", result[1].GoodLocationName);
        Assert.AreEqual("PAL-A", result[1].PalletCode);
        Assert.AreEqual("LOC-C", result[2].GoodLocationName);
        Assert.AreEqual("PAL-B", result[2].PalletCode);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_CancellationRequested_ThrowsOperationCanceled()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 10);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => sut.GetLocationStockBySkuAsync(request, user, cts.Token));
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_MultipleStockRecordsSameLocationAndPallet_AggregatesQty()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 10, palletCode: "PAL-001");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 25, palletCode: "PAL-001");

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(35, result[0].Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_NullPalletCode_TreatedAsEmptyString()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 15, palletCode: null);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("", result[0].PalletCode);
        Assert.AreEqual(15, result[0].Items![0].Qty);
    }

    [TestMethod]
    public async Task GetLocationStockBySkuAsync_LockedQtyExceedsStock_StillReturnsNegativeAvailable()
    {
        SeedLocation(tenantId: 1, id: 1, warehouseId: 5, locationName: "LOC-A");
        SeedSku(id: 100, skuName: "Widget");
        SeedStock(tenantId: 1, goodsLocationId: 1, skuId: 100, qty: 10);

        SeedDispatchWithPick(tenantId: 1, dispatchId: 1, dispatchStatus: 3,
            pickId: 1, skuId: 100, goodsLocationId: 1, pickQty: 50);

        var sut = CreateSut();
        var user = new CurrentUser { tenant_id = 1 };
        var request = new GetLocationStockBySkuRequest { WarehouseId = 5, SkuId = 100 };

        var result = await sut.GetLocationStockBySkuAsync(request, user, CancellationToken.None);

        // The method does not filter out negative available, it returns the calculated value
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result[0].Items![0].Qty < 0);
    }

    #endregion

    #region Helpers

    private StockService CreateSut()
    {
        return new StockService(_dbContext, _localizerMock.Object, _loggerMock.Object, _configurationMock.Object);
    }

    private void SeedLocation(long tenantId, int id, int warehouseId, string locationName,
        bool isValid = true, byte warehouseAreaProperty = 0)
    {
        _dbContext.GetDbSet<GoodslocationEntity>().Add(new GoodslocationEntity
        {
            Id = id,
            TenantId = tenantId,
            WarehouseId = warehouseId,
            LocationName = locationName,
            WarehouseName = "WH",
            WarehouseAreaName = "Area-A",
            IsValid = isValid,
            WarehouseAreaProperty = warehouseAreaProperty,
            CoordinateZ = "1"
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

    private void SeedStock(long tenantId, int goodsLocationId, int skuId, int qty,
        bool isFreeze = false, string? palletCode = null)
    {
        _dbContext.GetDbSet<StockEntity>().Add(new StockEntity
        {
            TenantId = tenantId,
            goods_location_id = goodsLocationId,
            sku_id = skuId,
            qty = qty,
            is_freeze = isFreeze,
            Palletcode = palletCode
        });
        _dbContext.SaveChanges();
    }

    private void SeedDispatchWithPick(long tenantId, int dispatchId, byte dispatchStatus,
        int pickId, int skuId, int goodsLocationId, int pickQty)
    {
        var dispatchSet = _dbContext.GetDbSet<DispatchlistEntity>();
        if (!dispatchSet.Any(d => d.Id == dispatchId))
        {
            dispatchSet.Add(new DispatchlistEntity
            {
                Id = dispatchId,
                TenantId = tenantId,
                dispatch_status = dispatchStatus,
                dispatch_no = $"DSP-{dispatchId}",
                customer_name = "Test"
            });
            _dbContext.SaveChanges();
        }

        _dbContext.GetDbSet<DispatchpicklistEntity>().Add(new DispatchpicklistEntity
        {
            Id = pickId,
            dispatchlist_id = dispatchId,
            sku_id = skuId,
            goods_location_id = goodsLocationId,
            pick_qty = pickQty
        });
        _dbContext.SaveChanges();
    }

    #endregion
}
