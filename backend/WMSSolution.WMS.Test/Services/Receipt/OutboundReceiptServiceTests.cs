using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;
using WMSSolution.WMS.Services.Receipt;

namespace WMSSolution.WMS.Test.Services.Receipt;

[TestClass]
public class OutboundReceiptServiceTests
{
    private SqlDBContext _context = null!;
    private Mock<IStringLocalizer<MultiLanguage>> _mockLocalizer = null!;
    private Mock<ILogger<OutboundReceiptService>> _mockLogger = null!;
    private Mock<FunctionHelper> _mockFunctionHelper = null!;
    private OutboundReceiptService _service = null!;
    private CurrentUser _currentUser = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<SqlDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new SqlDBContext(options);

        _mockLocalizer = new Mock<IStringLocalizer<MultiLanguage>>();
        _mockLocalizer
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _mockLogger = new Mock<ILogger<OutboundReceiptService>>();

        var mockAccessor = new Mock<IHttpContextAccessor>();
        var mockTokenSettings = Options.Create(new TokenSettings { SigningKey = "test-key-at-least-32-characters-long!" });
        _mockFunctionHelper = new Mock<FunctionHelper>(_context, mockAccessor.Object, mockTokenSettings) { CallBase = false };

        _service = new OutboundReceiptService(
            _context,
            _mockLocalizer.Object,
            _mockLogger.Object,
            _mockFunctionHelper.Object);

        _currentUser = new CurrentUser
        {
            user_id = 1,
            user_name = "TestUser",
            tenant_id = 1
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
    }

    #region Seed Helpers

    private void SeedUom(int uomId = 1, int skuId = 1, bool isBaseUnit = true, int conversionRate = 1)
    {
        var uomEntity = new SkuUomEntity
        {
            Id = uomId,
            UnitName = "EA",
            ConversionRate = conversionRate,
            IsBaseUnit = isBaseUnit,
            Operator = ConversionOperator.Multiply,
            TenantId = 1
        };
        _context.GetDbSet<SkuUomEntity>().Add(uomEntity);

        var uomLinkEntity = new SkuUomLinkEntity
        {
            SkuId = skuId,
            SkuUomId = uomId,
        };
        _context.GetDbSet<SkuUomLinkEntity>().Add(uomLinkEntity);

        // 3. Lưu thay đổi vào database
        _context.SaveChanges();
    }

    private void SeedStock(int skuId, int locationId, int qty, string palletCode = "", long tenantId = 1)
    {
        _context.GetDbSet<StockEntity>().Add(new StockEntity
        {
            sku_id = skuId,
            goods_location_id = locationId,
            qty = qty,
            actual_qty = qty,
            Palletcode = palletCode,
            TenantId = tenantId,
            goods_owner_id = 1,
            last_update_time = DateTime.UtcNow,
            PutAwayDate = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    private void SeedLocation(int id, byte status = (byte)GoodLocationStatusEnum.OCCUPIED, long tenantId = 1)
    {
        _context.GetDbSet<GoodslocationEntity>().Add(new GoodslocationEntity
        {
            Id = id,
            WarehouseId = 1,
            LocationName = $"LOC-{id}",
            WarehouseName = "WH-01",
            LocationStatus = status,
            IsValid = true,
            TenantId = tenantId,
            GoodsLocationType = GoodsLocationTypeEnum.StorageSlot
        });
        _context.SaveChanges();
    }

    private void SeedPallet(string palletCode, long tenantId = 1)
    {
        _context.GetDbSet<PalletEntity>().Add(new PalletEntity
        {
            PalletCode = palletCode,
            PalletStatus = PalletEnumStatus.InUse,
            IsFull = true,
            TenantId = tenantId
        });
        _context.SaveChanges();
    }

    private static CreateOutboundReceiptRequest CreateValidRequest(
        decimal quantity = 5,
        string palletCode = "PL-001")
    {
        return new CreateOutboundReceiptRequest
        {
            ReceiptNo = "PXK-TEST-001",
            WarehouseId = 1,
            OutboundGatewayId = 1,
            CustomerId = 1,
            Type = OutboundTypeConstant.Outbound,
            CreatedDate = DateTime.UtcNow,
            Details =
            [
                new CreateOutboundReceiptDetailDto
                {
                    SkuId = 1,
                    Quantity = quantity,
                    SkuUomId = 1,
                    LocationId = 1,
                    PalletCode = palletCode
                }
            ]
        };
    }

    /// <summary>
    /// Seed all base data needed for a happy-path test
    /// </summary>
    private void SeedHappyPathData(int stockQty = 5)
    {
        SeedUom();
        SeedStock(skuId: 1, locationId: 1, qty: stockQty, palletCode: "PL-001");
        SeedLocation(id: 1, status: (byte)GoodLocationStatusEnum.OCCUPIED);
        SeedPallet("PL-001");
    }

    #endregion

    // ═══════════════════════════════════════════════════════════
    //  HAPPY CASE
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Test: Valid request, sufficient stock, full deduction
    /// → Receipt created, stock = 0, location → AVAILABLE, pallet → Available
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_ValidRequest_ShouldCreateReceiptAndDeductStock()
    {
        // Arrange
        SeedHappyPathData(stockQty: 5);
        var request = CreateValidRequest(quantity: 5);

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert — receipt created
        Assert.IsTrue(id > 0, "Receipt ID should be greater than 0");
        Assert.AreEqual("save_success", message);

        var saved = await _context.GetDbSet<OutBoundReceiptEntity>()
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id);

        Assert.IsNotNull(saved);
        Assert.AreEqual("PXK-TEST-001", saved.ReceiptNumber);
        Assert.AreEqual(1, saved.WarehouseId);
        Assert.AreEqual(1, saved.CustomerId);
        Assert.AreEqual("TestUser", saved.Creator);
        Assert.AreEqual(1, saved.Details.Count);

        // Assert — stock deducted to 0
        var stock = await _context.GetDbSet<StockEntity>()
            .FirstOrDefaultAsync(s => s.sku_id == 1 && s.goods_location_id == 1);

        Assert.IsNotNull(stock);
        Assert.AreEqual(0, stock.qty, "Stock qty should be 0 after full deduction");
        Assert.AreEqual(0m, stock.actual_qty, "Stock actual_qty should be 0 after full deduction");

        // Assert — location released
        var location = await _context.GetDbSet<GoodslocationEntity>()
            .FirstOrDefaultAsync(l => l.Id == 1);

        Assert.IsNotNull(location);
        Assert.AreEqual((byte)GoodLocationStatusEnum.AVAILABLE, location.LocationStatus,
            "Location should be AVAILABLE when no stock remains");

        // Assert — pallet released
        var pallet = await _context.GetDbSet<PalletEntity>()
            .FirstOrDefaultAsync(p => p.PalletCode == "PL-001");

        Assert.IsNotNull(pallet);
        Assert.AreEqual(PalletEnumStatus.Available, pallet.PalletStatus,
            "Pallet should be Available when no stock remains");
        Assert.IsFalse(pallet.IsFull);
    }

    /// <summary>
    /// Test: Empty details → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_EmptyDetails_ShouldReturnError()
    {
        var request = CreateValidRequest();
        request.Details = [];

        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        Assert.AreEqual(0, id);
        Assert.AreEqual("Details are required", message);

        var count = await _context.GetDbSet<OutBoundReceiptEntity>().CountAsync();
        Assert.AreEqual(0, count, "No receipt should be saved in DB");
    }

    /// <summary>
    /// Test: WarehouseId = 0 → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_InvalidWarehouseId_ShouldReturnError()
    {
        var request = CreateValidRequest();
        request.WarehouseId = 0;

        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        Assert.AreEqual(0, id);
        Assert.AreEqual("Warehouse is required", message);
    }

    /// <summary>
    /// Test: OutboundGatewayId = 0 → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_InvalidGatewayId_ShouldReturnError()
    {
        var request = CreateValidRequest();
        request.OutboundGatewayId = 0;

        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        Assert.AreEqual(0, id);
        Assert.AreEqual("Outbound gateway is required", message);
    }

    /// <summary>
    /// Test: Empty receipt number → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_EmptyReceiptNo_ShouldReturnError()
    {
        var request = CreateValidRequest();
        request.ReceiptNo = "";

        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        Assert.AreEqual(0, id);
        Assert.AreEqual("Receipt number is required", message);
    }

    /// <summary>
    /// Test: Invalid detail (SkuId = 0) → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_InvalidDetailItem_ShouldReturnError()
    {
        var request = CreateValidRequest();
        request.Details[0].SkuId = 0;

        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        Assert.AreEqual(0, id);
        Assert.AreEqual("Invalid detail item", message);
    }

    /// <summary>
    /// Test: Missing LocationId → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_MissingLocationId_ShouldReturnError()
    {
        var request = CreateValidRequest();
        request.Details[0].LocationId = null;

        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        Assert.AreEqual(0, id);
        Assert.AreEqual("Location is required for outbound", message);
    }

    /// <summary>
    /// Test: Quantity = 0 → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_ZeroQuantity_ShouldReturnError()
    {
        var request = CreateValidRequest();
        request.Details[0].Quantity = 0;

        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        Assert.AreEqual(0, id);
        Assert.AreEqual("Invalid detail item", message);
    }

    // ═══════════════════════════════════════════════════════════
    //  DUPLICATE RECEIPT NUMBER
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Test: Duplicate receipt number → return error, only first receipt exists
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_DuplicateReceiptNo_ShouldReturnError()
    {
        // Arrange — seed enough stock for 2 requests
        SeedUom();
        SeedStock(skuId: 1, locationId: 1, qty: 100, palletCode: "PL-001");
        SeedLocation(id: 1);
        SeedPallet("PL-001");

        var request1 = CreateValidRequest();
        await _service.CreateAsync(request1, _currentUser, CancellationToken.None);

        // Act — second request with same receipt number
        var request2 = CreateValidRequest();

        var (id, message) = await _service.CreateAsync(request2, _currentUser, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, id);
        Assert.AreEqual("Receipt number already exists", message);

        var count = await _context.GetDbSet<OutBoundReceiptEntity>().CountAsync();
        Assert.AreEqual(1, count, "Only the first receipt should exist");
    }

    // ═══════════════════════════════════════════════════════════
    //  INSUFFICIENT STOCK
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Test: Stock has 3, request needs 5 → return error, stock unchanged
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_InsufficientStock_ShouldReturnError()
    {
        // Arrange — stock = 3, request = 5
        SeedUom();
        SeedStock(skuId: 1, locationId: 1, qty: 3, palletCode: "PL-001");
        SeedLocation(id: 1);

        var request = CreateValidRequest(quantity: 5);

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, id);
        Assert.IsTrue(message.Contains("Insufficient stock"),
            $"Expected insufficient stock message, got: {message}");

        // Stock unchanged
        var stock = await _context.GetDbSet<StockEntity>().FirstAsync();
        Assert.AreEqual(3, stock.qty, "Stock should remain unchanged when validation fails");
        Assert.AreEqual(3m, stock.actual_qty);
    }

    /// <summary>
    /// Test: No stock record at all → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_StockNotFound_ShouldReturnError()
    {
        SeedUom();
        SeedLocation(id: 1);

        var request = CreateValidRequest();

        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        Assert.AreEqual(0, id);
        Assert.IsTrue(message.Contains("Insufficient stock"));
    }

    // ═══════════════════════════════════════════════════════════
    //  PARTIAL DEDUCTION — location & pallet stay occupied
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Test: Stock has 10, request takes 3 → stock = 7, location stays OCCUPIED, pallet stays InUse
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_PartialDeduction_ShouldKeepLocationAndPalletOccupied()
    {
        // Arrange — stock = 10, take 3
        SeedHappyPathData(stockQty: 10);
        var request = CreateValidRequest(quantity: 3);

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert — receipt created
        Assert.IsTrue(id > 0);
        Assert.AreEqual("save_success", message);

        // Assert — stock partially deducted
        var stock = await _context.GetDbSet<StockEntity>().FirstAsync();
        Assert.AreEqual(7, stock.qty);
        Assert.AreEqual(7m, stock.actual_qty);

        // Assert — location stays OCCUPIED
        var location = await _context.GetDbSet<GoodslocationEntity>().FirstAsync(l => l.Id == 1);
        Assert.AreEqual((byte)GoodLocationStatusEnum.OCCUPIED, location.LocationStatus,
            "Location should remain OCCUPIED when stock remains");

        // Assert — pallet stays InUse
        var pallet = await _context.GetDbSet<PalletEntity>().FirstAsync(p => p.PalletCode == "PL-001");
        Assert.AreEqual(PalletEnumStatus.InUse, pallet.PalletStatus,
            "Pallet should remain InUse when stock remains");
    }

    // ═══════════════════════════════════════════════════════════
    //  MULTI-SKU SAME LOCATION — release only when ALL empty
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Test: Location has 2 SKUs, deduct only SKU-1 to 0 → location stays OCCUPIED
    /// because SKU-2 still has stock
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_MultiSkuSameLocation_ShouldNotReleaseIfOtherSkuRemains()
    {
        // Arrange — SKU-1 qty=5 and SKU-2 qty=10 at location 1
        SeedUom();
        SeedStock(skuId: 1, locationId: 1, qty: 5, palletCode: "PL-001");
        SeedStock(skuId: 2, locationId: 1, qty: 10, palletCode: "PL-002");
        SeedLocation(id: 1, status: (byte)GoodLocationStatusEnum.OCCUPIED);
        SeedPallet("PL-001");

        var request = CreateValidRequest(quantity: 5); // take all of SKU-1

        // Act
        var (id, _) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert — receipt created
        Assert.IsTrue(id > 0);

        // Location stays OCCUPIED because SKU-2 still has stock
        var location = await _context.GetDbSet<GoodslocationEntity>().FirstAsync(l => l.Id == 1);
        Assert.AreEqual((byte)GoodLocationStatusEnum.OCCUPIED, location.LocationStatus,
            "Location should remain OCCUPIED when other SKU still has stock at this location");
    }

    // ═══════════════════════════════════════════════════════════
    //  NO PALLET CODE — pallet release should be skipped
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Test: Request without pallet code → receipt created, no pallet release logic triggered
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_NoPalletCode_ShouldNotReleasePallet()
    {
        // Arrange — stock without pallet
        SeedUom();
        SeedStock(skuId: 1, locationId: 1, qty: 5, palletCode: "");
        SeedLocation(id: 1, status: (byte)GoodLocationStatusEnum.OCCUPIED);
        SeedPallet("PL-UNRELATED"); // unrelated pallet

        var request = CreateValidRequest(quantity: 5, palletCode: "");

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert — receipt created
        Assert.IsTrue(id > 0);
        Assert.AreEqual("save_success", message);

        // Unrelated pallet should remain InUse
        var pallet = await _context.GetDbSet<PalletEntity>()
            .FirstAsync(p => p.PalletCode == "PL-UNRELATED");
        Assert.AreEqual(PalletEnumStatus.InUse, pallet.PalletStatus,
            "Unrelated pallet should not be affected");
    }

    // ═══════════════════════════════════════════════════════════
    //  UOM CONVERSION — non-base unit
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Test: Request 2 boxes (1 box = 5 EA) → deduct 10 EA from stock
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_WithUomConversion_ShouldDeductConvertedQty()
    {
        // Arrange — 1 box = 5 EA (multiply)
        SeedUom(uomId: 10, skuId: 1, isBaseUnit: false, conversionRate: 5);
        SeedStock(skuId: 1, locationId: 1, qty: 20, palletCode: "PL-001");
        SeedLocation(id: 1, status: (byte)GoodLocationStatusEnum.OCCUPIED);
        SeedPallet("PL-001");

        var request = CreateValidRequest(quantity: 2); // 2 boxes
        request.Details[0].SkuUomId = 10; // box UOM

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert — receipt created
        Assert.IsTrue(id > 0);
        Assert.AreEqual("save_success", message);

        // Stock should be 20 - (2 * 5) = 10
        var stock = await _context.GetDbSet<StockEntity>().FirstAsync();
        Assert.AreEqual(10, stock.qty, "Should deduct 10 EA (2 boxes × 5 EA/box)");
        Assert.AreEqual(10m, stock.actual_qty);
    }
}
