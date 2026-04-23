using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Models.IntegrationWCS;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Asn.Asnmaster;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Inbound;
using WMSSolution.WMS.IServices.IntegrationWCS;
using WMSSolution.WMS.Services.Asn;

namespace WMSSolution.WMS.Test.Services.Asn;

[TestClass]
public class AsnDraftSubmitTests
{
    private SqlDBContext _dbContext = null!;
    private FunctionHelper _functionHelper = null!;
    private Mock<IStringLocalizer<MultiLanguage>> _localizerMock = null!;
    private Mock<ILogger<AsnService>> _loggerMock = null!;
    private Mock<IIntegrationService> _integrationServiceMock = null!;
    private Exception? _lastLoggedError;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<SqlDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            // InMemory provider does not support transactions; ignore warning to allow code paths using BeginTransactionAsync()
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new SqlDBContext(options);

        // Seed base UOM for SkuId=1 to satisfy receipt detail creation
        var spu = new SpuEntity { Id = 1, TenantId = 1, spu_code = "SPU001", spu_name = "SPU" };
        var sku = new SkuEntity { Id = 1, spu_id = 1, sku_code = "SKU001", sku_name = "SKU", Spu = spu };
        _dbContext.GetDbSet<SpuEntity>().Add(spu);
        _dbContext.GetDbSet<SkuEntity>().Add(sku);
        var uom = new SkuUomEntity
        {
            Id = 1,
            UnitName = "EA",
            ConversionRate = 1,
            IsBaseUnit = true,
            TenantId = 1
        };
        _dbContext.GetDbSet<SkuUomEntity>().Add(uom);

        var uomLink = new SkuUomLinkEntity
        {
            SkuId = sku.Id,
            SkuUomId = uom.Id,
        };
        _dbContext.GetDbSet<SkuUomLinkEntity>().Add(uomLink);

        _dbContext.SaveChanges();

        _localizerMock = new Mock<IStringLocalizer<MultiLanguage>>();
        _localizerMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _loggerMock = new Mock<ILogger<AsnService>>();
        _loggerMock
            .Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
            .Callback((LogLevel _, EventId _, object _, Exception ex, object _) => _lastLoggedError = ex);
        _integrationServiceMock = new Mock<IIntegrationService>();

        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var tokenSettings = Options.Create(new TokenSettings
        {
            SigningKey = "01234567890123456789012345678901",
            Issuer = "WMSSolution",
            Audience = "WMSSolution",
            ExpireMinute = 60
        });
        _functionHelper = new FunctionHelper(_dbContext, httpContextAccessor, tokenSettings);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task SubmitOrderAsync_ShouldCreateAsnMasterAndReceipt()
    {
        var sut = new AsnService(
            _dbContext,
            _localizerMock.Object,
            _functionHelper,
            _loggerMock.Object,
            _integrationServiceMock.Object);

        _integrationServiceMock
            .Setup(s => s.CreateInboundEntitiesAsync(It.IsAny<List<CreateInboundTaskDTO>>(), It.IsAny<CurrentUser>()))
            .ReturnsAsync(
            [
                new InboundEntity { PalletCode = "PALLET1", LocationId = 1, TenantId = 1 }
            ]);
        _integrationServiceMock
            .Setup(s => s.CreateIntegrationHistoryAsync(It.IsAny<List<InboundEntity>>(), It.IsAny<CurrentUser>()))
            .ReturnsAsync(true);

        var currentUser = new CurrentUser { user_name = "tester", tenant_id = 1 };
        var viewModel = CreateMinimalAsnmasterBothViewModel();
        viewModel.detailList[0].goods_location_id = 1;
        viewModel.detailList[0].batch_number = "PALLET1";

        var (id, msg) = await sut.SubmitOrderAsync(viewModel, currentUser);

        if (id <= 0)
        {
            Assert.Fail(_lastLoggedError?.ToString() ?? msg);
        }

        var receipts = await _dbContext.GetDbSet<InboundReceiptEntity>().ToListAsync();
        Assert.AreEqual(1, receipts.Count);
        Assert.IsFalse(string.IsNullOrWhiteSpace(receipts[0].ReceiptNumber));
    }

    [TestMethod]
    public async Task SubmitOrderAsync_WhenIntegrationThrows_ShouldReturnError()
    {
        _integrationServiceMock
            .Setup(s => s.CreateInboundEntitiesAsync(It.IsAny<List<CreateInboundTaskDTO>>(), It.IsAny<CurrentUser>()))
            .ThrowsAsync(new InvalidOperationException("Integration failed"));

        var sut = new AsnService(
            _dbContext,
            _localizerMock.Object,
            _functionHelper,
            _loggerMock.Object,
            _integrationServiceMock.Object);

        var currentUser = new CurrentUser { user_name = "tester", tenant_id = 1 };
        var viewModel = CreateMinimalAsnmasterBothViewModel();
        viewModel.detailList[0].goods_location_id = 1;
        viewModel.detailList[0].batch_number = "PALLET1";

        var (id, msg) = await sut.SubmitOrderAsync(viewModel, currentUser);

        Assert.AreEqual(0, id);
        Assert.IsFalse(string.IsNullOrWhiteSpace(msg));
    }

    private static AsnmasterBothViewModel CreateMinimalAsnmasterBothViewModel()
    {
        return new AsnmasterBothViewModel
        {
            goods_owner_id = 1,
            goods_owner_name = "GO",
            estimated_arrival_time = DateTime.UtcNow.AddDays(1),
            detailList =
            [
                new AsnmasterDetailViewModel
                {
                    spu_id = 1,
                    spu_code = "SPU001",
                    spu_name = "SPU",
                    sku_id = 1,
                    sku_code = "SKU001",
                    sku_name = "SKU",
                    supplier_id = 2,
                    supplier_name = "SUP",
                    asn_qty_decimal = 1,
                    actual_qty_decimal = 1
                }
            ]
        };
    }

    // NOTE: transaction/rollback behavior is covered by integration failures via IIntegrationService when needed.
}

