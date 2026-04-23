using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples;
using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.InventoryStrategy;
using WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;
namespace WMSSolution.WMS.Test.Services.WarehousePrinciples;

[TestClass]
public class LIFO_PrinciplesTests : WarehousePrinciplesTests
{
    [TestInitialize]
    public void Setup()
    {
        _principles = new WarehouseManagementPrinciples(_inventory, new LifoRetrievalStrategy());
    }

    [TestMethod]
    public void GetItemsByCategory_LIFO_ReturnList()
    {
        // Arrange - No data in database
        // Act
        var results = _principles.GetItemsByCategory(_category);
        var items = _inventory.Where(x => x.Category == _category)
            .OrderByDescending(i => i.ReceivedDate).ToList();

        var ids = string.Join("-", items.Select(x => x.Id));
        var rsIds = string.Join("-", results.Select(x => x.Id));

        // Assert
        Assert.IsNotNull(results);
        Assert.HasCount(items.Count, results);
        Assert.AreEqual(ids, rsIds);
    }

    [TestMethod]
    public void GetItemsByQuantity_LIFO_ReturnList()
    {
        // Arrange - No data in database
        // Act
        int qty = random.Next(23, 100);
        var results = _principles.GetItemsByQuantity(_category, qty);

        var data = _inventory.Where(i => i.Category == _category)
            .OrderByDescending(i => i.ReceivedDate)
            .ToList();

        var listItems = new List<InventoryItem>();
        int quantity = 0;
        foreach (var item in data)
        {
            quantity += item.Quantity;
            if (quantity >= qty)
            {
                listItems.Add(item);
                break;
            }

            listItems.Add(item);
        }

        var ids = string.Join("-", listItems.Select(x => x.Id));
        var rsIds = string.Join("-", results.Select(x => x.Id));
        // Assert
        Assert.IsNotNull(results);
        Assert.HasCount(listItems.Count, results);
        Assert.AreEqual(ids, rsIds);
    }

    [TestMethod]
    public void GetItems_LIFO_ReturnList()
    {
        // Arrange - No data in database
        // Act
        int qty = random.Next(23, 100);
        var supplier = _suppliers[qty % 5];
        var results = _principles.GetItems(_category, qty, supplier.Id);

        var data = _inventory.Where(i => i.Category == _category
                && i.SupplierId == supplier.Id)
            .OrderByDescending(i => i.ReceivedDate)
            .ToList();

        var listItems = new List<InventoryItem>();
        int quantity = 0;
        foreach (var item in data)
        {
            quantity += item.Quantity;
            if (quantity >= qty)
            {
                listItems.Add(item);
                break;
            }

            listItems.Add(item);
        }

        var ids = string.Join("-", listItems.Select(x => x.Id));
        var rsIds = string.Join("-", results.Select(x => x.Id));

        // Assert
        Assert.IsNotNull(results);
        Assert.HasCount(listItems.Count, results);
        Assert.AreEqual(ids, rsIds);
    }
}