using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples;
using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

namespace WMSSolution.WMS.Test.Services.WarehousePrinciples;

public class OrchestratorPrinciples
{
    protected WarehouseOrchestratorPrinciples _principles = null!;
    protected List<InventoryLocation> _locations = [];
    protected List<InventoryRuleSettings> _ruleSettings = [];
    protected readonly List<Supplier> _suppliers = [];
    protected readonly List<CategoryItem> _categories = [];
    protected readonly List<SkuDetail> _skus = [];
    protected readonly List<PalletItem> _pallets = [];
    protected readonly Random random = new();
    protected readonly List<int> _trackingPallets = [];
    
    protected readonly int sampleBlockId = 1;
    protected readonly int sampleCategoryId = 1;
    protected readonly int sampleFloorId = 1;
    protected readonly int sampleSkuId = 1;

    private readonly int _totalSupplier = 10;
    private readonly int _totalSKU = 10;
    private readonly int _totalBlock = 1;
    private readonly int _totalFloor = 2;
    private readonly int _totalCategory = 5;
    private readonly int _totalWarehouse = 2;
    private readonly int _totalPallet = 100;
    private readonly int _maxCoordX = 5;
    private readonly int _maxCoordY = 5;
    private readonly int _maxQuantity = 500;
    private readonly int _maxQtyPerPallet = 100;

    public OrchestratorPrinciples()
    {
        for (int i = 0; i < _totalSupplier; i++)
        {
            _suppliers.Add(new Supplier
            {
                Id = i + 1,
                Name = $"Supplier {i + 1}",
                ContactInfo = $"contact{i + 1}"
            });
        }

        for (int i = 0; i < _totalCategory; i++)
        {
            _categories.Add(new CategoryItem
            {
                Id = i + 1,
                CategoryName = $"Category {i + 1}"
            });
        }

        for (int i = 0; i < _totalSKU; i++)
        {
            _skus.Add(new SkuDetail
            {
                Id = i + 1,
                SkuName = $"SKU {i + 1}",
                CategoryId = random.Next(1, _totalCategory),
                MaxQuantityPerPallet = _maxQtyPerPallet,
            });
        }

        for (int i = 0; i < _totalPallet; i++)
        {
            _pallets.Add(new PalletItem
            {
                Id = i + 1,
                PalletCode = $"Pallet {i + 1}",
                IsFull = random.Next(0, 2) == 0,
                MixedPallet = random.Next(0, 2) == 1,
                Details =
                [
                    new PalletDetail
                    {
                        SkuId = random.Next(1, _totalSKU),
                        Quantity = random.Next(10, _maxQuantity),
                        MaxQuantity = _maxQuantity,
                        ExpirationDate = DateTime.UtcNow.AddMonths(random.Next(1, 6))
                    },
                    new PalletDetail
                    {
                        SkuId = random.Next(1, _totalSKU),
                        Quantity = random.Next(10, _maxQuantity),
                        MaxQuantity = _maxQuantity,
                        ExpirationDate = DateTime.UtcNow.AddMonths(random.Next(1, 6))
                    }
                ]
            });
        }
    }

    protected List<InventoryLocation> InitDataLocations()
    {
        var locations = new List<InventoryLocation>();
        int idx = 1;
        for (int i = 1; i <= _totalWarehouse; i++)
        {
            for (int floorId = 1; floorId <= _totalFloor; floorId++)
            {
                for (int blockId = 1; blockId <= _totalBlock; blockId++)
                {
                    List<InventoryLocation> blockLocations = RandomBlockLocations(floorId, blockId, idx);
                    idx += blockLocations.Count;
                    locations.AddRange(blockLocations);
                }
            }
        }

        return locations;
    }

    private List<InventoryLocation> RandomBlockLocations(int floorId, int blockId, int idx)
    {
        var inventories = new List<InventoryLocation>();
        int i = idx;
        for (int coordX = 0; coordX < random.Next(1, _maxCoordX); coordX++)
        {
            for (int coordY = 0; coordY < random.Next(1, _maxCoordY); coordY++)
            {
                var palletId = RandomPallet();
                if (palletId.GetValueOrDefault() > 0)
                {
                    if (_trackingPallets.Any(x => x == palletId)) palletId = null;
                    else _trackingPallets.Add(palletId.GetValueOrDefault());
                }

                inventories.Add(new InventoryLocation
                {
                    Id = i + 1,
                    LocationName = $"Location_{i + 1}",
                    WarehouseId = random.Next(1, _totalWarehouse),
                    BlockId = blockId,
                    FloorId = floorId,
                    CoordinateX = coordX,
                    CoordinateY = coordY,
                    CoordinateZ = floorId,
                    PalletId = palletId,
                    Priority = random.Next(1, _maxCoordX)
                });
            }
        }

        return inventories;
    }

    private int? RandomPallet()
    {
        return random.Next(0, 2) == 1 ? null : random.Next(1, _totalPallet);
    }

    protected List<InventoryRuleSettings> InitSample_FIFO_Settings()
    {
        var settings = new List<InventoryRuleSettings>();
        for (int i = 0; i < 5; i++)
        {
            settings.Add(new InventoryRuleSettings
            {
                RuleName = $"Rule_{i + 1}",
                RuleSettings = OperationRules.FIFO,
                WarehouseId = random.Next(1, _totalWarehouse),
                SupplierIds = [1, 2],
                Details = CreateSampleDetails(),
                CreatedDate = DateTime.Now.AddDays(-random.Next(1, 30))
            });
        }

        return settings;
    }

    protected List<InventoryLocation> InitSample_FIFO_Locations()
    {
        var inventories = new List<InventoryLocation>();
        int i = 1;

        for (int coordX = 0; coordX < _maxCoordX; coordX++)
        {
            for (int coordY = 0; coordY < random.Next(1, _maxCoordY); coordY++)
            {
                inventories.Add(new InventoryLocation
                {
                    Id = i++,
                    LocationName = $"Location_{coordX + 1}.{coordY + 1}",
                    WarehouseId = random.Next(1, _totalWarehouse),
                    BlockId = sampleBlockId,
                    FloorId = sampleFloorId,
                    CoordinateX = coordX,
                    CoordinateY = coordY,
                    CoordinateZ = sampleFloorId,
                    Priority = random.Next(1, _maxCoordX)
                });
            }
        }

        return inventories;
    }

    private IEnumerable<RuleDetail> CreateSampleDetails()
    {
        return [
            new RuleDetail
            {
                BlockId = sampleBlockId,
                CategoryId  = sampleCategoryId,
                FloorId = sampleFloorId,
                SkuId = sampleSkuId
            }
        ];
    }

    protected List<InventoryRuleSettings> InitDataSettings()
    {
        var settings = new List<InventoryRuleSettings>();
        for (int i = 0; i < 5; i++)
        {
            settings.Add(new InventoryRuleSettings
            {
                RuleName = $"Rule_{i + 1}",
                RuleSettings = RandomRuleSettings(),
                WarehouseId = random.Next(1, _totalWarehouse),
                SupplierIds = [random.Next(1, _totalSupplier), random.Next(1, _totalSupplier)],
                Details = CreateDetails(),
                CreatedDate = DateTime.Now.AddDays(-random.Next(1, 30))
            });
        }

        return settings;
    }

    private OperationRules RandomRuleSettings()
    {
        int idx = random.Next(10, 50);

        if (idx % 3 == 1) return OperationRules.FIFO;
        if (idx % 3 == 2) return OperationRules.LIFO;

        return OperationRules.FEFO;
    }

    protected IEnumerable<RuleDetail> CreateDetails()
    {
        var details = new List<RuleDetail>();
        for (int i = 0; i < random.Next(1, 10); i++)
        {
            int idx = random.Next(10, 50);
            details.Add(new RuleDetail
            {
                SkuId = random.Next(1, _totalSKU),
                BlockId = random.Next(1, _totalBlock),
                FloorId = random.Next(1, _totalFloor),
                CategoryId = idx % 3 == 0 ? null : random.Next(1, _totalCategory)
            });
        }

        return details;
    }

    protected InboundGoods SampleInboundGoods()
    {
        return new InboundGoods
        {
            WarehouseId = random.Next(1, _totalWarehouse),
            PurchaseOrderDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(random.Next(1, 3)),
            ReceivingDate = DateTime.UtcNow,
            Details =
            [
                new() {
                    SkuId = sampleSkuId,
                    Quantity = 145,
                    SupplierId = 1,
                    ExpirationDate = DateTime.Now.AddMonths(2)
                },
                new() {
                    SkuId = sampleSkuId,
                    Quantity = 145,
                    SupplierId = 1,
                    ExpirationDate = DateTime.Now.AddMonths(random.Next(3, 6))
                },
                new() {
                    SkuId = sampleSkuId,
                    Quantity = 450,
                    SupplierId = 2,
                    ExpirationDate = DateTime.Now.AddMonths(random.Next(3, 6))
                },
                new() {
                    SkuId = sampleSkuId,
                    Quantity = 145,
                    SupplierId = 1,
                    ExpirationDate = DateTime.Now.AddMonths(7)
                },
            ]
        };
    }

    protected InboundGoods SampleNoPlanYet()
    {
        var details = new List<InboundDetail>();

        for (int i = 0; i < 20; i++)
        {
            details.Add(new()
            {
                SkuId = random.Next(1, _totalSKU),
                Quantity = random.Next(100, 500),
                SupplierId = random.Next(1, _totalSupplier),
                ExpirationDate = DateTime.Now.AddMonths(random.Next(3, 6))
            });
        }

        return new InboundGoods
        {
            WarehouseId = random.Next(1, _totalWarehouse),
            PurchaseOrderDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(random.Next(1, 3)),
            ReceivingDate = DateTime.UtcNow,
            Details = details
        };
    }

    protected InboundGoods RandomInboundGoods()
    {
        return new InboundGoods
        {
            WarehouseId = random.Next(1, _totalWarehouse),
            PurchaseOrderDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(random.Next(1, 3)),
            ReceivingDate = DateTime.UtcNow,
            Details =
            [
                new() {
                    SkuId = random.Next(1, _totalSKU),
                    Quantity = random.Next(100, 500),
                    SupplierId = random.Next(1, _totalSupplier),
                    ExpirationDate = DateTime.Now.AddMonths(random.Next(3, 6))
                },
                new() {
                    SkuId = random.Next(1, _totalSKU),
                    Quantity = random.Next(100, 500),
                    SupplierId = random.Next(1, _totalSupplier),
                    ExpirationDate = DateTime.Now.AddMonths(random.Next(3, 6))
                }
            ]
        };
    }
}