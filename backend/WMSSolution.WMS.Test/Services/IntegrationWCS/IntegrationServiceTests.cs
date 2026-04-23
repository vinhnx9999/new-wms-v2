//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Localization;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Moq;
//using WMSSolution.Core;
//using WMSSolution.Core.DBContext;
//using WMSSolution.Core.JWT;
//using WMSSolution.Core.Models;
//using WMSSolution.Core.Models.IntegrationWCS;
//using WMSSolution.Shared.Enums;
//using WMSSolution.WMS.Entities.Models;
//using WMSSolution.WMS.Services.IntegrationWCS;

//namespace WMSSolution.WMS.Test.Services.IntegrationWCS;

//[TestClass]
//public class IntegrationServiceTests
//{
//    private SqlDBContext _dbContext = null!;
//    private Mock<IStringLocalizer<MultiLanguage>> _localizerMock = null!;
//    private Mock<ILogger<IntegrationService>> _loggerMock = null!;
//    private IntegrationService _service = null!;
//    private string _apiUrl = "https://localhost:7077";

//    [TestInitialize]
//    public void Setup()
//    {
//        var options = new DbContextOptionsBuilder<SqlDBContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        _dbContext = new SqlDBContext(options);
//        _localizerMock = new Mock<IStringLocalizer<MultiLanguage>>();
//        _loggerMock = new Mock<ILogger<IntegrationService>>();
//        var serviceProvider = new Mock<IServiceProvider>();
//        var httpClientFactory = new Mock<IHttpClientFactory>();
//        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
//        var tokenSettings = Options.Create(new TokenSettings
//        {
//            Audience = "test",
//            Issuer = "test",
//            SigningKey = "test-signing-key",
//            ExpireMinute = 60
//        });
//        var functionHelper = new FunctionHelper(_dbContext, httpContextAccessor, tokenSettings);

//        // Mock IConfiguration
//        var inMemorySettings = new Dictionary<string, string> {
//            {"WcsSettings:ApiUrl", _apiUrl}
//        };

//        IConfiguration configuration = new ConfigurationBuilder()
//                .AddInMemoryCollection(inMemorySettings)
//                .Build();

//        serviceProvider
//            .Setup(sp => sp.GetService(typeof(IConfiguration)))
//            .Returns(configuration);

//        httpClientFactory
//            .Setup(f => f.CreateClient(It.IsAny<string>()))
//            .Returns(new HttpClient());


//        _service = new IntegrationService(_dbContext, httpClientFactory.Object,
//            _localizerMock.Object, _loggerMock.Object, serviceProvider.Object, functionHelper);
//    }

//    [TestCleanup]
//    public void Cleanup()
//    {
//        _dbContext.Dispose();
//    }

//    #region GetInboundTaskAsync Tests

//    [TestMethod]
//    public async Task GetInboundTaskAsync_WhenNoInbounds_ReturnsEmptyList()
//    {
//        //// Arrange - No data in database

//        //// Act
//        //var result = await _service.GetInboundTaskAsync();

//        //// Assert
//        //Assert.IsNotNull(result);
//        //Assert.IsFalse(result.Any());
//    }

//    [TestMethod]
//    public async Task GetInboundTaskAsync_WhenAllInboundsDone_ReturnsEmptyList()
//    {
//        //// Arrange
//        //_dbContext.Inbounds.Add(new InboundEntity
//        //{
//        //    Id = 1,
//        //    PalletCode = "PLT001",
//        //    Status = IntegrationStatus.Done, // All done - should be excluded
//        //    LocationId = 1,
//        //    TaskCode = "TASK001"
//        //});
//        //await _dbContext.SaveChangesAsync();

//        //// Act
//        //var result = await _service.GetInboundTaskAsync();

//        //// Assert
//        //Assert.IsNotNull(result);
//        //Assert.IsFalse(result.Any());
//    }

//    [TestMethod]
//    public async Task GetInboundTaskAsync_WhenInboundsExist_ReturnsCorrectData()
//    {
//        //// Arrange
//        //var location = new GoodslocationEntity
//        //{
//        //    Id = 1,
//        //    CoordinateX = "10",
//        //    CoordinateY = "20",
//        //    CoordinateZ = "5",
//        //    WarehouseAreaName = "Block A",
//        //    IsValid = true,
//        //    LocationStatus = 1
//        //};
//        //_dbContext.GetDbSet<GoodslocationEntity>().Add(location);

//        //var inbound = new InboundEntity
//        //{
//        //    Id = 1,
//        //    PalletCode = "PLT001",
//        //    Status = IntegrationStatus.Processing,
//        //    LocationId = 1,
//        //    TaskCode = "TASK001",
//        //    PickUpDate = new DateTime(2026, 1, 12, 10, 0, 0)
//        //};
//        //_dbContext.Inbounds.Add(inbound);

//        //await _dbContext.SaveChangesAsync();

//        //// Act
//        //var result = await _service.GetInboundTaskAsync();

//        //// Assert
//        //Assert.IsNotNull(result);
//        //Assert.AreEqual(1, result.Count());

//        //var firstResult = result.First();
//        //Assert.AreEqual("TASK001", firstResult.TaskCode);
//        //Assert.AreEqual(1, firstResult.Details.Count());

//        //var detail = firstResult.Details.First();
//        //Assert.AreEqual("PLT001", detail.PalletCode);
//        //Assert.AreEqual("Processing", detail.Status);
//        //Assert.AreEqual("5.10.20", detail.Location);
//        //Assert.AreEqual("Block A", detail.BlockName);
//    }

//    [TestMethod]
//    public async Task GetInboundTaskAsync_WhenMultipleInboundsSameTaskCode_GroupsCorrectly()
//    {
//        // Arrange
//        var location1 = new GoodslocationEntity
//        {
//            Id = 1,
//            CoordinateX = "10",
//            CoordinateY = "20",
//            CoordinateZ = "5",
//            WarehouseAreaName = "Block A",
//            IsValid = true
//        };
//        var location2 = new GoodslocationEntity
//        {
//            Id = 2,
//            CoordinateX = "15",
//            CoordinateY = "25",
//            CoordinateZ = "6",
//            WarehouseAreaName = "Block B",
//            IsValid = true
//        };
//        _dbContext.GetDbSet<GoodslocationEntity>().AddRange(location1, location2);

//        // Two inbounds with same TaskCode
//        _dbContext.Inbounds.AddRange(
//            new InboundEntity
//            {
//                Id = 1,
//                PalletCode = "PLT001",
//                Status = IntegrationStatus.Processing,
//                LocationId = 1,
//                TaskCode = "TASK001"
//            },
//            new InboundEntity
//            {
//                Id = 2,
//                PalletCode = "PLT002",
//                Status = IntegrationStatus.Processing,
//                LocationId = 2,
//                TaskCode = "TASK001" // Same TaskCode
//            }
//        );
//        await _dbContext.SaveChangesAsync();

//        // Act
//        var result = await _service.GetInboundTaskAsync();

//        // Assert
//        Assert.AreEqual(1, result.Count()); // Should be grouped into 1 response
//        var response = result.First();
//        Assert.AreEqual("TASK001", response.TaskCode);
//        Assert.AreEqual(2, response.Details.Count()); // Should have 2 details
//    }

//    [TestMethod]
//    public async Task GetInboundTaskAsync_WhenDifferentTaskCodes_ReturnsMultipleResponses()
//    {
//        // Arrange
//        _dbContext.Inbounds.AddRange(
//            new InboundEntity
//            {
//                Id = 1,
//                PalletCode = "PLT001",
//                Status = IntegrationStatus.Processing,
//                LocationId = 1,
//                TaskCode = "TASK001"
//            },
//            new InboundEntity
//            {
//                Id = 2,
//                PalletCode = "PLT002",
//                Status = IntegrationStatus.Processing,
//                LocationId = 1,
//                TaskCode = "TASK002" // Different TaskCode
//            }
//        );
//        await _dbContext.SaveChangesAsync();

//        // Act
//        var result = await _service.GetInboundTaskAsync();

//        // Assert
//        Assert.AreEqual(2, result.Count()); // Should have 2 separate responses
//        Assert.IsTrue(result.Any(r => r.TaskCode == "TASK001"));
//        Assert.IsTrue(result.Any(r => r.TaskCode == "TASK002"));
//    }

//    [TestMethod]
//    public async Task GetInboundTaskAsync_WhenLocationNotFound_ReturnsEmptyLocation()
//    {
//        // Arrange - Inbound with non-existent LocationId
//        _dbContext.Inbounds.Add(new InboundEntity
//        {
//            Id = 1,
//            PalletCode = "PLT001",
//            Status = IntegrationStatus.Processing,
//            LocationId = 999, // Location doesn't exist
//            TaskCode = "TASK001"
//        });
//        await _dbContext.SaveChangesAsync();

//        // Act
//        var result = await _service.GetInboundTaskAsync();

//        // Assert
//        Assert.AreEqual(1, result.Count());
//        var detail = result.First().Details.First();
//        Assert.AreEqual(string.Empty, detail.Location);
//        Assert.AreEqual(string.Empty, detail.BlockName);
//    }

//    [TestMethod]
//    public async Task GetInboundTaskAsync_WithAsnData_ReturnsCorrectAsnInfo()
//    {
//        // Arrange
//        var sku = new SkuEntity
//        {
//            Id = 1,
//            sku_code = "SKU001",
//            sku_name = "Test SKU"
//        };
//        _dbContext.GetDbSet<SkuEntity>().Add(sku);

//        var asn = new AsnEntity
//        {
//            Id = 1,
//            asn_no = "ASN001",
//            goods_owner_name = "Test Owner",
//            sku_id = 1,
//            asn_status = 1
//        };
//        _dbContext.GetDbSet<AsnEntity>().Add(asn);

//        var asnSort = new AsnsortEntity
//        {
//            Id = 1,
//            asn_id = 1,
//            series_number = "PLT001",
//            sorted_qty = 10
//        };
//        _dbContext.GetDbSet<AsnsortEntity>().Add(asnSort);

//        _dbContext.Inbounds.Add(new InboundEntity
//        {
//            Id = 1,
//            PalletCode = "PLT001",
//            Status = IntegrationStatus.Processing,
//            LocationId = 1,
//            TaskCode = "TASK001"
//        });

//        await _dbContext.SaveChangesAsync();

//        // Act
//        var result = await _service.GetInboundTaskAsync();

//        // Assert
//        Assert.AreEqual(1, result.Count());
//        var response = result.First();
//        Assert.AreEqual("ASN001", response.AsnNumber);
//        Assert.AreEqual("Test Owner", response.GoodOwnerName);
//        Assert.AreEqual("SKU001", response.SkuCode);
//    }

//    [TestMethod]
//    public async Task GetInboundTaskAsync_ExcludesOnlyDoneStatus()
//    {
//        // Arrange - Add inbounds with different statuses
//        _dbContext.Inbounds.AddRange(
//            new InboundEntity { Id = 1, PalletCode = "PLT001", Status = IntegrationStatus.Processing, TaskCode = "T1" },
//            new InboundEntity { Id = 2, PalletCode = "PLT002", Status = IntegrationStatus.Processing, TaskCode = "T2" },
//            new InboundEntity { Id = 3, PalletCode = "PLT003", Status = IntegrationStatus.Done, TaskCode = "T3" } // Should be excluded
//        );
//        await _dbContext.SaveChangesAsync();

//        // Act
//        var result = await _service.GetInboundTaskAsync();

//        // Assert
//        Assert.AreEqual(2, result.Count()); // Only New and Pending
//        Assert.IsFalse(result.Any(r => r.Details.Any(d => d.Status == "Done")));
//    }

//    #endregion

//    [TestMethod]
//    public async Task GetMapLocations_ReturnList()
//    {
//        // Arrange - No data in database
//        // Act
//        var result = await _service.GetMapLocations(null, default);

//        // Assert
//        Assert.IsNotNull(result);
//        Assert.IsFalse(result.Any());
//    }
//}
