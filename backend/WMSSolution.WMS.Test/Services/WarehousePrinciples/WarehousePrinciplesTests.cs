using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples;
using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;
namespace WMSSolution.WMS.Test.Services.WarehousePrinciples;

public class WarehousePrinciplesTests
{
    protected readonly string _category = "Default Category";
    protected readonly List<Supplier> _suppliers = [];
    protected readonly Random random = new();
    protected List<InventoryItem> _inventory = [];
    protected WarehouseManagementPrinciples _principles = null!;

    public WarehousePrinciplesTests()
    {
        var data = new List<InventoryItem>();
        for (int i = 0; i < 6; i++)
        {
            _suppliers.Add(new Supplier
            {
                Id = i + 1,
                Name = $"Supplier {i + 1}",
                ContactInfo = $"contact{i + 1}"
            });
        }

        for (int i = 0; i < random.Next(100, 500); i++)
        {
            int idx = random.Next(0, 100);
            var supplier = _suppliers[idx % 5];
            data.Add(new InventoryItem
            {
                Id = i + 1,
                Category = idx % 3 == 1 ? $"{Guid.NewGuid()}" : _category,
                ExpirationDate = DateTime.UtcNow.AddDays(random.Next(10, 100)),
                ReceivedDate = DateTime.UtcNow.AddDays(-1 * random.Next(15, 30)),
                Name = $"{Guid.NewGuid()}",
                Quantity = random.Next(10, 500),
                ReorderLevel = random.Next(10, 50),
                SupplierId = supplier.Id,
                Supplier = supplier
            });
        }

        // Arrange - No data in database
        _inventory = data;
    }
}