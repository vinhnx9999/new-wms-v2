namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

/// <summary>
/// Plan Receiving
/// </summary>
public class PlanAccuracyLocation : InventoryLocation
{
    /// <summary>
    /// Accuracy Location
    /// </summary>
    public PlanAccuracyLocation()
    {

    }

    /// <summary>
    /// Accuracy Location
    /// </summary>
    /// <param name="location"></param>
    /// <param name="automationRule"></param>
    public PlanAccuracyLocation(InventoryLocation location, OperationRules automationRule)
    {
        this.Id = location.Id;
        this.LocationName = location.LocationName;
        this.WarehouseId = location.WarehouseId;
        this.BlockId = location.BlockId;
        this.FloorId = location.FloorId;
        this.CoordinateX = location.CoordinateX;
        this.CoordinateY = location.CoordinateY;
        this.CoordinateZ = location.CoordinateZ;
        this.Priority = location.Priority;
        this.PalletId = location.PalletId;
        this.Pallet = location.Pallet;
        this.AutomationRule = automationRule;
    }
    /// <summary>
    /// Sku Id
    /// </summary>
    public int SkuId { get; set; } = 0;
    /// <summary>
    /// Planned Quantity
    /// </summary>
    public int PlannedQuantity { get; set; } = 0;
    /// <summary>
    /// Supplier Id
    /// </summary>
    public int SupplierId { get; set; } = 0;
    /// <summary>
    /// Automation Rule
    /// </summary>
    public OperationRules AutomationRule { get; set; } = OperationRules.FEFO;
}

/// <summary>
/// Inventory Location
/// </summary>
public class InventoryLocation
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Location Name
    /// </summary>
    public string LocationName { get; set; } = "";
    /// <summary>
    /// WarehouseId
    /// </summary>
    public int WarehouseId { get; set; } = 1;
    /// <summary>
    /// Block
    /// </summary>
    public int? BlockId { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the floor associated with the entity.
    /// </summary>
    /// <remarks>The value can be null, indicating that the entity is not assigned to any specific floor.
    /// Check for null before performing operations that require a valid floor identifier.</remarks>
    public int? FloorId { get; set; }
    /// <summary>
    /// CoordinateX
    /// </summary>
    public int CoordinateX { get; set; } = 1;
    /// <summary>
    /// CoordinateY
    /// </summary>
    public int CoordinateY { get; set; } = 1;
    /// <summary>
    /// CoordinateZ
    /// </summary>
    public int CoordinateZ { get; set; } = 1;
    /// <summary>
    /// Priority
    /// </summary>
    public int Priority { get; set; } = 1;
    /// <summary>
    /// Pallet Id
    /// </summary>
    public int? PalletId { get; set; }
    /// <summary>
    /// Pallet
    /// </summary>
    public PalletItem Pallet { get; set; } = new PalletItem();
}

/// <summary>
/// SkuDetail
/// </summary>
public class SkuDetail
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Sku Name
    /// </summary>
    public string SkuName { get; set; } = "";
    /// <summary>
    /// Category Id
    /// </summary>
    public int CategoryId { get; set; }
    /// <summary>
    /// Gets or sets the maximum number of items that can be placed on a single pallet.
    /// </summary>
    /// <remarks>This property defines the upper limit for the quantity of items that may be stacked on one
    /// pallet. Ensure that the value assigned does not exceed the physical or safety constraints of the pallet in
    /// use.</remarks>
    public int? MaxQuantityPerPallet { get; set; } = 100;
}

/// <summary>
/// 
/// </summary>
public class PalletItem
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Pallet Name
    /// </summary>
    public string PalletCode { get; set; } = "";
    /// <summary>
    /// Details
    /// </summary>
    public IEnumerable<PalletDetail> Details { get; set; } = [];
    /// <summary>
    /// Is Full Pallet
    /// </summary>
    public bool IsFull { get; set; } = false;
    /// <summary>
    /// Multiple SKUs combined on the same pallet.
    /// </summary>
    public bool MixedPallet { get; set; } = true;
}

/// <summary>
/// Pallet Detail
/// </summary>
public class PalletDetail
{    
    /// <summary>
    /// Quantity
    /// </summary>
    public int Quantity { get; set; } = 0;
    /// <summary>
    /// Gets or sets the maximum quantity allowed for the item.
    /// </summary>
    /// <remarks>This property defines the upper limit of how many units can be allocated or processed. It is
    /// important to ensure that the value is non-negative.</remarks>
    public int MaxQuantity { get; set; } = 100;
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; }
    /// <summary>
    /// Gets or sets the expiration date and time for the item.
    /// </summary>
    /// <remarks>Use this property to determine when the item is no longer considered valid. Ensure that the
    /// expiration date is set appropriately to prevent the use of expired items.</remarks>
    public DateTime ExpirationDate { get; set; }
}

/// <summary>
/// Category
/// </summary>
public class CategoryItem
{
    /// <summary>
    /// Category Id
    /// </summary>
    public int Id { get; set; } = 1;
    /// <summary>
    /// Category Name
    /// </summary>
    public string CategoryName { get; set; } = "";
}
