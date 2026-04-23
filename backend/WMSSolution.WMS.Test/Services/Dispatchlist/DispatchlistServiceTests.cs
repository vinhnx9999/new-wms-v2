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
using WMSSolution.Shared.Enums.Outbound;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Dispatchlist;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Dispatchlist.Duy_Phat_Solution;
using WMSSolution.WMS.Services.Dispatchlist;

namespace WMSSolution.WMS.Test.Services.Dispatchlist
{
    [TestClass]
    public class DispatchlistServiceTests
    {
        private SqlDBContext _context;
        private Mock<IStringLocalizer<MultiLanguage>> _mockLocalizer;
        private FunctionHelper _functionHelper;
        private Mock<ILogger<DispatchlistService>> _mockLogger;
        private Mock<IServiceProvider> _mockServiceProvider;
        private DispatchlistService _service;
        private CurrentUser _currentUser;

        [TestInitialize]
        public void Setup()
        {
            // Setup in-memory database with transaction warning suppressed
            var options = new DbContextOptionsBuilder<SqlDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new SqlDBContext(options);

            // Setup mocks
            _mockLocalizer = new Mock<IStringLocalizer<MultiLanguage>>();
            _mockLogger = new Mock<ILogger<DispatchlistService>>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            // Setup FunctionHelper with real instance
            var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            var tokenSettings = Options.Create(new TokenSettings
            {
                Audience = "test",
                Issuer = "test",
                SigningKey = "test-signing-key-with-minimum-length-requirement",
                ExpireMinute = 60
            });
            _functionHelper = new FunctionHelper(_context, httpContextAccessor, tokenSettings);

            // Setup localizer default returns
            _mockLocalizer.Setup(l => l["sku_not_exist"]).Returns(new LocalizedString("sku_not_exist", "SKU does not exist"));
            _mockLocalizer.Setup(l => l["save_success"]).Returns(new LocalizedString("save_success", "Save successful"));
            _mockLocalizer.Setup(l => l["save_failed"]).Returns(new LocalizedString("save_failed", "Save failed"));

            // Create service
            _service = new DispatchlistService(
                _context,
                _mockLocalizer.Object,
                _functionHelper,
                _mockLogger.Object,
                _mockServiceProvider.Object
            );

            // Setup current user
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

        [TestMethod]
        public async Task AddAsync_WithValidData_ShouldReturnSuccess()
        {
            // Arrange

            var skuList = new List<SkuEntity> {
               new SkuEntity
            {
                Id = 1,
                sku_code = "SKU001",
                volume = 10.5m,
                weight = 5.2m,
                spu_id = 1
            },
                   new SkuEntity
            {
                Id = 2,
                sku_code = "SKU002",
                volume = 10.5m,
                weight = 5.2m,
                spu_id = 1
            },

           };
            await _context.GetDbSet<SkuEntity>().AddRangeAsync(skuList);
            await _context.SaveChangesAsync();

            var viewModels = new List<DispatchlistAddViewModel>
            {
                new DispatchlistAddViewModel
                {
                    sku_id = 1,
                    qty = 10,
                    customer_id = 1,
                    customer_name = "Customer A"
                },
                new DispatchlistAddViewModel
                {
                    sku_id = 2 ,
                    qty = 5,
                    customer_id = 1,
                    customer_name = "Customer B"
                }
            };

            // Act
            var result = await _service.AddAsync(viewModels, _currentUser);

            // Assert
            Assert.IsTrue(result.flag);
            Assert.AreEqual("Save successful", result.msg);

            var savedEntity = await _context.GetDbSet<DispatchlistEntity>().FirstOrDefaultAsync();
            Assert.IsNotNull(savedEntity);
            Assert.AreEqual(1, savedEntity.sku_id);
            Assert.AreEqual(10, savedEntity.qty);
            Assert.AreEqual(105m, savedEntity.volume); // 10 * 10.5
            Assert.AreEqual(52m, savedEntity.weight); // 10 * 5.2
            Assert.AreEqual("TestUser", savedEntity.creator);
            Assert.AreEqual(1, savedEntity.TenantId);
        }

        [TestMethod]
        public async Task AddAsync_WithMultipleItems_ShouldCreateAllEntities()
        {
            // Arrange
            var sku1 = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            var sku2 = new SkuEntity { Id = 2, sku_code = "SKU002", volume = 20m, weight = 10m, spu_id = 2 };
            _context.GetDbSet<SkuEntity>().AddRange(sku1, sku2);
            await _context.SaveChangesAsync();

            var viewModels = new List<DispatchlistAddViewModel>
            {
                new DispatchlistAddViewModel { sku_id = 1, qty = 5, customer_id = 1, customer_name = "Customer A" },
                new DispatchlistAddViewModel { sku_id = 2, qty = 3, customer_id = 1, customer_name = "Customer A" }
            };

            // Act
            var result = await _service.AddAsync(viewModels, _currentUser);

            // Assert
            Assert.IsTrue(result.flag);
            var savedEntities = await _context.GetDbSet<DispatchlistEntity>().ToListAsync();
            Assert.AreEqual(2, savedEntities.Count);
            Assert.IsTrue(savedEntities.All(e => !string.IsNullOrEmpty(e.dispatch_no)));
            // All items should have the same dispatch_no
            Assert.AreEqual(savedEntities[0].dispatch_no, savedEntities[1].dispatch_no);
        }

        [TestMethod]
        public async Task AddAsync_WithMissingSkuIds_ShouldReturnFailure()
        {
            // Arrange
            var viewModels = new List<DispatchlistAddViewModel>
            {
                new DispatchlistAddViewModel
                {
                    sku_id = 999, // Non-existent SKU ID
                    qty = 10,
                    customer_id = 1,
                    customer_name = "Customer A"
                }
            };

            // Act
            var result = await _service.AddAsync(viewModels, _currentUser);

            // Assert
            Assert.IsFalse(result.flag);
            Assert.AreEqual("SKU does not exist", result.msg);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("SKU IDs do not exist")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task AddAsync_WithEmptyList_ShouldReturnFailure()
        {
            // Arrange
            var viewModels = new List<DispatchlistAddViewModel>();

            // Act
            var result = await _service.AddAsync(viewModels, _currentUser);

            // Assert
            // When list is empty, SaveChanges returns 0, so the service returns false
            Assert.IsFalse(result.flag);
            Assert.AreEqual("Save failed", result.msg);
            var savedEntities = await _context.GetDbSet<DispatchlistEntity>().ToListAsync();
            Assert.AreEqual(0, savedEntities.Count);
        }

        [TestMethod]
        public async Task AddAsync_WithNullSkuVolume_ShouldHandleGracefully()
        {
            // Arrange
            var skuEntity = new SkuEntity
            {
                Id = 1,
                sku_code = "SKU001",
                volume = 0,
                weight = 0,
                spu_id = 1
            };
            _context.GetDbSet<SkuEntity>().Add(skuEntity);
            await _context.SaveChangesAsync();

            var viewModels = new List<DispatchlistAddViewModel>
            {
                new DispatchlistAddViewModel
                {
                    sku_id = 1,
                    qty = 10,
                    customer_id = 1,
                    customer_name = "Customer A"
                }
            };

            // Act
            var result = await _service.AddAsync(viewModels, _currentUser);

            // Assert
            Assert.IsTrue(result.flag);
            var savedEntity = await _context.GetDbSet<DispatchlistEntity>().FirstOrDefaultAsync();
            Assert.IsNotNull(savedEntity);
            Assert.AreEqual(0, savedEntity.volume);
            Assert.AreEqual(0, savedEntity.weight);
        }

        [TestMethod]
        public async Task AddAsync_WhenSaveChangesFails_ShouldReturnFailure()
        {
            // Arrange
            var skuEntity = new SkuEntity
            {
                Id = 1,
                sku_code = "SKU001",
                volume = 10m,
                weight = 5m,
                spu_id = 1
            };
            _context.GetDbSet<SkuEntity>().Add(skuEntity);
            await _context.SaveChangesAsync();

            var viewModels = new List<DispatchlistAddViewModel>
            {
                new DispatchlistAddViewModel
                {
                    sku_id = 1,
                    qty = 10,
                    customer_id = 1,
                    customer_name = "Customer A"
                }
            };

            // Dispose context to simulate save failure
            await _context.DisposeAsync();

            // Act
            var result = await _service.AddAsync(viewModels, _currentUser);

            // Assert
            Assert.IsFalse(result.flag);
            Assert.AreEqual("Save failed", result.msg);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error in AddAsync")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task AddAsync_WithDuplicateSkuIds_ShouldCreateMultipleRecords()
        {
            // Arrange
            var skuEntity = new SkuEntity
            {
                Id = 1,
                sku_code = "SKU001",
                volume = 10m,
                weight = 5m,
                spu_id = 1
            };
            _context.GetDbSet<SkuEntity>().Add(skuEntity);
            await _context.SaveChangesAsync();

            var viewModels = new List<DispatchlistAddViewModel>
            {
                new DispatchlistAddViewModel { sku_id = 1, qty = 5, customer_id = 1, customer_name = "Customer A" },
                new DispatchlistAddViewModel { sku_id = 1, qty = 10, customer_id = 1, customer_name = "Customer A" }
            };

            // Act
            var result = await _service.AddAsync(viewModels, _currentUser);

            // Assert
            Assert.IsTrue(result.flag);
            var savedEntities = await _context.GetDbSet<DispatchlistEntity>().ToListAsync();
            Assert.AreEqual(2, savedEntities.Count);
            Assert.AreEqual(5, savedEntities[0].qty);
            Assert.AreEqual(10, savedEntities[1].qty);
        }

        [TestMethod]
        public async Task AddAsync_ShouldSetCorrectTimestamps()
        {
            // Arrange
            var skuEntity = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().Add(skuEntity);
            await _context.SaveChangesAsync();

            var viewModels = new List<DispatchlistAddViewModel>
            {
                new DispatchlistAddViewModel { sku_id = 1, qty = 10, customer_id = 1, customer_name = "Customer A" }
            };

            var beforeTime = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var result = await _service.AddAsync(viewModels, _currentUser);

            var afterTime = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.IsTrue(result.flag);
            var savedEntity = await _context.GetDbSet<DispatchlistEntity>().FirstOrDefaultAsync();
            Assert.IsNotNull(savedEntity);
            Assert.IsTrue(savedEntity.create_time >= beforeTime && savedEntity.create_time <= afterTime);
            Assert.IsTrue(savedEntity.last_update_time >= beforeTime && savedEntity.last_update_time <= afterTime);
        }

        [TestMethod]
        public async Task AddAsync_WithException_ShouldLogAndReturnFailure()
        {
            // Arrange
            var viewModels = new List<DispatchlistAddViewModel>
            {
                new DispatchlistAddViewModel { sku_id = 1, qty = 10, customer_id = 1, customer_name = "Customer A" }
            };

            // Dispose context to trigger exception
            await _context.DisposeAsync();

            // Act
            var result = await _service.AddAsync(viewModels, _currentUser);

            // Assert
            Assert.IsFalse(result.flag);
            Assert.AreEqual("Save failed", result.msg);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error in AddAsync")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #region CreateDispatchDraftAsync Tests

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithNullRequest_ShouldReturnError()
        {
            // Arrange
            CreateDispatchOrderRequest? request = null;

            // Act
            var result = await _service.CreateDispatchDraftAsync(request!);

            // Assert
            Assert.AreEqual(0, result.dispatchId);
            Assert.AreEqual("Request is required", result.message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Request is null")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithEmptyLines_ShouldReturnError()
        {
            // Arrange
            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>()
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.AreEqual(0, result.dispatchId);
            Assert.AreEqual("At least one line item is required", result.message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("No lines in request")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithNullLines_ShouldReturnError()
        {
            // Arrange
            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = null!
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.AreEqual(0, result.dispatchId);
            Assert.AreEqual("At least one line item is required", result.message);
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithNonExistentSkuIds_ShouldReturnError()
        {
            // Arrange
            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest
                    {
                        SkuId = 999, // Non-existent
                        UomId = 1,
                        ReqQty = 10,
                        Description = "Test"
                    }
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.AreEqual(0, result.dispatchId);
            Assert.IsTrue(result.message.Contains("SKU(s) not found"));
            Assert.IsTrue(result.message.Contains("999"));
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithValidData_ShouldCreateHeaderAndDetails()
        {
            // Arrange
            var sku = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().Add(sku);
            await _context.SaveChangesAsync();

            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest
                    {
                        SkuId = 1,
                        UomId = 1,
                        ReqQty = 10,
                        Description = "Test line"
                    }
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.IsTrue(result.dispatchId > 0);
            Assert.IsTrue(result.message.Contains("created successfully"));

            // Verify header created
            var header = await _context.GetDbSet<DispatchlistEntity>()
                .FirstOrDefaultAsync(h => h.Id == result.dispatchId);
            Assert.IsNotNull(header);
            Assert.AreEqual("Test Customer", header.customer_name);
            Assert.AreEqual(1, header.customer_id);
            Assert.AreEqual(0, header.dispatch_status);

            // Verify details created
            var details = await _context.GetDbSet<DispatchListDetailEntity>()
                .Where(d => d.dispatchlist_id == result.dispatchId)
                .ToListAsync();
            Assert.AreEqual(1, details.Count);
            Assert.AreEqual(1, details[0].sku_id);
            Assert.AreEqual(10, details[0].req_qty);
            Assert.AreEqual("Test line", details[0].description);
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithMultipleLines_ShouldCreateAllDetails()
        {
            // Arrange
            var sku1 = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            var sku2 = new SkuEntity { Id = 2, sku_code = "SKU002", volume = 20m, weight = 10m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().AddRange(sku1, sku2);
            await _context.SaveChangesAsync();

            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest { SkuId = 1, UomId = 1, ReqQty = 10, Description = "Line 1" },
                    new ManualDispatchLineRequest { SkuId = 2, UomId = 1, ReqQty = 20, Description = "Line 2" }
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.IsTrue(result.dispatchId > 0);

            var details = await _context.GetDbSet<DispatchListDetailEntity>()
                .Where(d => d.dispatchlist_id == result.dispatchId)
                .ToListAsync();
            Assert.AreEqual(2, details.Count);
            Assert.IsTrue(details.Any(d => d.sku_id == 1 && d.req_qty == 10));
            Assert.IsTrue(details.Any(d => d.sku_id == 2 && d.req_qty == 20));
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithSelectedLocations_ShouldCreatePicklists()
        {
            // Arrange
            var sku = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().Add(sku);
            await _context.SaveChangesAsync();

            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest
                    {
                        SkuId = 1,
                        UomId = 1,
                        ReqQty = 15,
                        Description = "Test line",
                        SelectedLocations = new List<ManualPickLocationRequest>
                        {
                            new ManualPickLocationRequest { LocationId = 101, PalletId = 1, PickQty = 10 },
                            new ManualPickLocationRequest { LocationId = 102, PalletId = 2, PickQty = 5 }
                        }
                    }
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.IsTrue(result.dispatchId > 0);

            var detail = await _context.GetDbSet<DispatchListDetailEntity>()
                .FirstOrDefaultAsync(d => d.dispatchlist_id == result.dispatchId);
            Assert.IsNotNull(detail);
            Assert.AreEqual(15, detail.allocated_qty); // Sum of PickQty

            var picklists = await _context.GetDbSet<DispatchpicklistEntity>()
                .Where(p => p.dispatch_detail_id == detail.Id)
                .ToListAsync();
            Assert.AreEqual(2, picklists.Count);
            Assert.IsTrue(picklists.Any(p => p.goods_location_id == 101 && p.pick_qty == 10));
            Assert.IsTrue(picklists.Any(p => p.goods_location_id == 102 && p.pick_qty == 5));
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithoutSelectedLocations_ShouldNotCreatePicklists()
        {
            // Arrange
            var sku = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().Add(sku);
            await _context.SaveChangesAsync();

            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest
                    {
                        SkuId = 1,
                        UomId = 1,
                        ReqQty = 10,
                        Description = "Test line",
                        SelectedLocations = null
                    }
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.IsTrue(result.dispatchId > 0);

            var detail = await _context.GetDbSet<DispatchListDetailEntity>()
                .FirstOrDefaultAsync(d => d.dispatchlist_id == result.dispatchId);
            Assert.IsNotNull(detail);
            Assert.AreEqual(0, detail.allocated_qty);

            var picklists = await _context.GetDbSet<DispatchpicklistEntity>()
                .Where(p => p.dispatch_detail_id == detail.Id)
                .ToListAsync();
            Assert.AreEqual(0, picklists.Count);
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithCustomDispatchNo_ShouldUseProvidedValue()
        {
            // Arrange
            var sku = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().Add(sku);
            await _context.SaveChangesAsync();

            var customDispatchNo = "CUSTOM-001";
            var request = new CreateDispatchOrderRequest
            {
                DispatchNo = customDispatchNo,
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest { SkuId = 1, UomId = 1, ReqQty = 10 }
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.IsTrue(result.dispatchId > 0);

            var header = await _context.GetDbSet<DispatchlistEntity>()
                .FirstOrDefaultAsync(h => h.Id == result.dispatchId);
            Assert.IsNotNull(header);
            Assert.AreEqual(customDispatchNo, header.dispatch_no);
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithEmptyDispatchNo_ShouldAutoGenerate()
        {
            // Arrange
            var sku = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().Add(sku);
            await _context.SaveChangesAsync();

            var request = new CreateDispatchOrderRequest
            {
                DispatchNo = "", // Empty - should auto-generate
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest { SkuId = 1, UomId = 1, ReqQty = 10 }
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.IsTrue(result.dispatchId > 0);

            var header = await _context.GetDbSet<DispatchlistEntity>()
                .FirstOrDefaultAsync(h => h.Id == result.dispatchId);
            Assert.IsNotNull(header);
            Assert.IsFalse(string.IsNullOrEmpty(header.dispatch_no));
            Assert.IsTrue(header.dispatch_no.Contains("Outbound")); // Based on GetFormNoAsync pattern
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_ShouldSetCorrectInitialStatus()
        {
            // Arrange
            var sku = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().Add(sku);
            await _context.SaveChangesAsync();

            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest
                    {
                        SkuId = 1,
                        UomId = 1,
                        ReqQty = 10,
                        SelectedLocations = new List<ManualPickLocationRequest>
                        {
                            new ManualPickLocationRequest { LocationId = 101, PalletId = 1, PickQty = 10 }
                        }
                    }
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            var header = await _context.GetDbSet<DispatchlistEntity>()
                .FirstOrDefaultAsync(h => h.Id == result.dispatchId);
            Assert.AreEqual(0, header!.dispatch_status); // Draft status

            var detail = await _context.GetDbSet<DispatchListDetailEntity>()
                .FirstOrDefaultAsync(d => d.dispatchlist_id == result.dispatchId);
            Assert.AreEqual(DispatchDetailStatus.Pending, detail!.status);

            var picklist = await _context.GetDbSet<DispatchpicklistEntity>()
                .FirstOrDefaultAsync(p => p.dispatch_detail_id == detail.Id);
            Assert.AreEqual(DispatchPickListStatus.Pending, picklist!.status);
            Assert.AreEqual(0, picklist.picked_qty);
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_WithMultipleMissingSkus_ShouldReturnAllMissing()
        {
            // Arrange
            var sku = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().Add(sku);
            await _context.SaveChangesAsync();

            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest { SkuId = 1, UomId = 1, ReqQty = 10 }, // Exists
                    new ManualDispatchLineRequest { SkuId = 999, UomId = 1, ReqQty = 5 }, // Missing
                    new ManualDispatchLineRequest { SkuId = 888, UomId = 1, ReqQty = 3 }  // Missing
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.AreEqual(0, result.dispatchId);
            Assert.IsTrue(result.message.Contains("999"));
            Assert.IsTrue(result.message.Contains("888"));
        }

        [TestMethod]
        public async Task CreateDispatchDraftAsync_ShouldCalculateAllocatedQtyFromLocations()
        {
            // Arrange
            var sku = new SkuEntity { Id = 1, sku_code = "SKU001", volume = 10m, weight = 5m, spu_id = 1 };
            _context.GetDbSet<SkuEntity>().Add(sku);
            await _context.SaveChangesAsync();

            var request = new CreateDispatchOrderRequest
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CreateDate = DateTime.UtcNow,
                Lines = new List<ManualDispatchLineRequest>
                {
                    new ManualDispatchLineRequest
                    {
                        SkuId = 1,
                        UomId = 1,
                        ReqQty = 100, // Requesting 100
                        SelectedLocations = new List<ManualPickLocationRequest>
                        {
                            new ManualPickLocationRequest { LocationId = 1, PalletId = 1, PickQty = 30 },
                            new ManualPickLocationRequest { LocationId = 2, PalletId = 2, PickQty = 25 },
                            new ManualPickLocationRequest { LocationId = 3, PalletId = 3, PickQty = 20 }
                        }
                    }
                }
            };

            // Act
            var result = await _service.CreateDispatchDraftAsync(request);

            // Assert
            Assert.IsTrue(result.dispatchId > 0);

            var detail = await _context.GetDbSet<DispatchListDetailEntity>()
                .FirstOrDefaultAsync(d => d.dispatchlist_id == result.dispatchId);
            Assert.IsNotNull(detail);
            Assert.AreEqual(100, detail.req_qty);
            Assert.AreEqual(75, detail.allocated_qty); // 30 + 25 + 20 = 75
        }

        #endregion
    }
}
