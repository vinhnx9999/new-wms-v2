using Hangfire;
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
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels.Receipt;
using WMSSolution.WMS.Services.Receipt;

namespace WMSSolution.WMS.Test.Services.Receipt;

[TestClass]
public class ReceiptServiceTests
{
    private SqlDBContext _context = null!;
    private Mock<IStringLocalizer<MultiLanguage>> _mockLocalizer = null!;
    private Mock<ILogger<ReceiptService>> _mockLogger = null!;
    private FunctionHelper _functionHelper = null!;
    private ReceiptService _service = null!;
    private CurrentUser _currentUser = null!;
    private Mock<IBackgroundJobClient> _mockBackgroudJob = null!;

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
        _mockLogger = new Mock<ILogger<ReceiptService>>();
        _mockBackgroudJob = new Mock<IBackgroundJobClient>();
        var mockAccessor = new Mock<IHttpContextAccessor>();

        var tokenSettings = Options.Create(new TokenSettings { SigningKey = "TokenSettings:SigningKey" });

        _functionHelper = new FunctionHelper(_context, mockAccessor.Object, tokenSettings);


        _service = new ReceiptService(
            _context,
            _mockLocalizer.Object,
            _mockLogger.Object,
            _functionHelper,
            _mockBackgroudJob.Object);

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

    #region Helper

    private static CreateReceiptRequest CreateValidRequest(bool isStored = false)
    {
        return new CreateReceiptRequest
        {
            ReceiptNo = "PNK-TEST-001",
            WarehouseId = 1,
            Type = InboundTypeConstant.Inbound,
            IsStored = isStored,
            CreatedDate = DateTime.UtcNow,
            SupplierId = 1,
            Description = "Test receipt",
            Details =
            [
                new CreateReceiptDetailDto
                {
                    SkuId = 1,
                    Quantity = 10,
                    SkuUomId = 1,
                    LocationId = isStored ? 1 : null,
                    PalletCode = isStored ? "PL-001" : null,
                    ExpiryDate = DateTime.UtcNow,
                },
                new CreateReceiptDetailDto
                {
                    SkuId = 2,
                    Quantity = 20,
                    SkuUomId = 1,
                    LocationId = isStored ? 2 : null,
                    PalletCode = isStored ? "PL-002" : null,
                    ExpiryDate = DateTime.UtcNow,
                }
            ]
        };
    }

    #endregion

    /// <summary>
    /// Test 1: Valid request, IsStored = false → create receipt successfully
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_ValidRequest_NotStored_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest(isStored: false);

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert
        Assert.IsGreaterThan(0, id, "Receipt ID should be greater than 0");
        Assert.AreEqual("save_success", message);

        var saved = await _context.GetDbSet<InboundReceiptEntity>()
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id);

        Assert.IsNotNull(saved);
        Assert.AreEqual("PNK-TEST-001", saved.ReceiptNumber);
        Assert.AreEqual(1, saved.WarehouseId);
        Assert.AreEqual("TestUser", saved.Creator);
        Assert.AreEqual(2, saved.Details.Count);
    }

    /// <summary>
    /// Test 2: Empty details → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_EmptyDetails_ShouldReturnError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Details = [];

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, id);
        Assert.AreEqual("Details are required", message);

        var count = await _context.GetDbSet<InboundReceiptEntity>().CountAsync();
        Assert.AreEqual(0, count, "No receipt should be saved in DB");
    }

    /// <summary>
    /// Test 3: WarehouseId = 0 → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_InvalidWarehouseId_ShouldReturnError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.WarehouseId = 0;

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, id);
        Assert.AreEqual("Warehouse is required", message);
    }

    /// <summary>
    /// Test 4: Duplicate receipt number → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_DuplicateReceiptNumber_ShouldReturnError()
    {
        // Arrange — create first receipt
        var request1 = CreateValidRequest();
        await _service.CreateAsync(request1, _currentUser, CancellationToken.None);

        // Create second receipt with same number
        var request2 = CreateValidRequest();

        // Act
        var (id, message) = await _service.CreateAsync(request2, _currentUser, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, id);
        Assert.AreEqual("Receipt number already exists", message);

        var count = await _context.GetDbSet<InboundReceiptEntity>().CountAsync();
        Assert.AreEqual(1, count, "Only the first receipt should exist");
    }

    /// <summary>
    /// Test 5: IsStored = true but LocationId = null → return error
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_IsStored_MissingLocation_ShouldReturnError()
    {
        // Arrange
        var request = CreateValidRequest(isStored: true);
        request.Details[0].LocationId = null; // missing location

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, id);
        Assert.AreEqual("Location is required when storing", message);
    }

    /// <summary>
    /// Test 6: IsStored = true, valid locations → create receipt + stock records
    /// Happy test case
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_ValidRequest_IsStored_ShouldCreateReceiptAndStock()
    {
        // Arrange — seed UOM data (UpdateStockAsync queries sku_uom)
        var uomDbSet = _context.GetDbSet<SkuUomEntity>();
        await uomDbSet.AddAsync(new SkuUomEntity
        {
            Id = 1,
            UnitName = "Cái",
            ConversionRate = 1,
            IsBaseUnit = true,
            Operator = ConversionOperator.Multiply,
            TenantId = 1
        });

        var uomLinkDbSet = _context.GetDbSet<SkuUomLinkEntity>();
        await uomLinkDbSet.AddAsync(new SkuUomLinkEntity
        {
            SkuId = 1,
            SkuUomId = 1,
        });

        await _context.SaveChangesAsync();

        var request = CreateValidRequest(isStored: true);

        // Act
        var (id, message) = await _service.CreateAsync(request, _currentUser, CancellationToken.None);

        // Assert — receipt created
        Assert.IsTrue(id > 0, "Receipt ID should be greater than 0");
        Assert.AreEqual("save_success", message);

        var saved = await _context.GetDbSet<InboundReceiptEntity>()
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id);

        Assert.IsNotNull(saved);
        Assert.AreEqual("PNK-TEST-001", saved.ReceiptNumber);
        Assert.AreEqual(2, saved.Details.Count);

        // Assert — stock records created
        var stockDbSet = _context.GetDbSet<StockEntity>();
        var stocks = await stockDbSet.ToListAsync();

        Assert.AreEqual(2, stocks.Count, "Should create 2 stock records");

        var stock1 = stocks.FirstOrDefault(s => s.sku_id == 1 && s.goods_location_id == 1);
        Assert.IsNotNull(stock1);
        Assert.AreEqual(10m, stock1.actual_qty, "SKU 1 should have qty 10 (base unit, no conversion)");
        Assert.AreEqual("PL-001", stock1.Palletcode);
        Assert.AreEqual(_currentUser.tenant_id, stock1.TenantId);

        var stock2 = stocks.FirstOrDefault(s => s.sku_id == 2 && s.goods_location_id == 2);
        Assert.IsNotNull(stock2);
        Assert.AreEqual(20m, stock2.actual_qty, "SKU 2 should have qty 20 (base unit, no conversion)");
        Assert.AreEqual("PL-002", stock2.Palletcode);
    }

    //[TestMethod]
    //public async Task UpdateAsync_ValidRequest_ShouldUpdateReceipt()
    //{
    //    // Arrange 
    //    var request = CreateValidRequest(isStored: false);
    //    var (id, createMessage) = await _service.CreateNewAsync(request, _currentUser, CancellationToken.None);

    //    var updateRequest = CreateValidRequest(isStored: false);
    //    updateRequest.Details[0].Quantity = 99;
    //    updateRequest.Details[0].LocationId = 99;
    //    updateRequest.Details[0].PalletCode = "PL-099";
    //    updateRequest.Description = "Updated";

    //    //// Act
    //    //var (success, message) = await _service.UpdateAsync(id, updateRequest, _currentUser, CancellationToken.None);

    //    //// Assert
    //    //Assert.IsTrue(success);

    //    //var updated = await _context.GetDbSet<InboundReceiptEntity>()
    //    //    .Include(r => r.Details)
    //    //    .FirstOrDefaultAsync(r => r.Id == id);

    //    //Assert.IsNotNull(updated);
    //    //Assert.AreEqual("Updated", updated.Description);
    //    //Assert.AreEqual(99, updated.Details.First().Quantity);
    //}

}
