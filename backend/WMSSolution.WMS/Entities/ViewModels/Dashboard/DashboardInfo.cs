using WMSSolution.Shared.Enums;

namespace WMSSolution.WMS.Entities.ViewModels.Dashboard;

/// <summary>
/// Dashboard Info
/// </summary>
public class DashboardInfo
{
    /// <summary>
    /// TotalWarehouses
    /// </summary>
    public int TotalWarehouses { get; set; }
    /// <summary>
    /// Total SKUs
    /// </summary>
    public int TotalItems { get; set; }
    /// <summary>
    /// Total Inventory locations
    /// </summary>
    public int TotalInventory { get; set; }
    /// <summary>
    /// Warehouse Capacity
    /// </summary>
    public int WarehouseCapacity { get; set; }
    /// <summary>
    /// Pending Orders
    /// </summary>
    public int PendingOrders { get; set; } = 0;
    /// <summary>
    /// Finish Orders
    /// </summary>
    public int ProcessingOrders { get; set; } = 0;
    /// <summary>
    /// Finish Orders has been Finish
    /// </summary>
    public int TodayOrders { get; set; } = 0;
    /// <summary>
    /// Order Item
    /// </summary>
    public IEnumerable<OrderItemDTO> Items { get; set; } = [];
    /// <summary>
    /// Low Inventory Alert
    /// </summary>
    public int LowInventoryAlert { get; set; } = 0;
    /// <summary>
    /// Yesterday Orders has been Finish
    /// </summary>
    public int YesterdayOrders { get; set; } = 0;
    /// <summary>
    /// Gets or sets the time filter applied to the dashboard data.
    /// </summary>
    /// <remarks>The default value is <see cref="FilterDashboardByTime.LastWeek"/>, which restricts the
    /// dashboard to display data from the previous week. Changing this property allows users to customize the time
    /// range for data visualization on the dashboard.</remarks>
    public FilterDashboardByTime FilterDashboard { get; set; } = FilterDashboardByTime.LastWeek;
    /// <summary>
    /// Inbound items by type 
    /// </summary>
    public IEnumerable<DateOrderItemDTO> InboundItems { get; set; } = [];
    /// <summary>
    /// Gets or sets the collection of outbound date order items.
    /// </summary>
    /// <remarks>This property holds the items that are scheduled for outbound processing. It is important to
    /// ensure that the collection is properly initialized before use.</remarks>
    public IEnumerable<DateOrderItemDTO> OutboundItems { get; set; } = [];

}

/// <summary>
/// Inventory info
/// </summary>
public class InventoryDTO
{
    /// <summary>
    /// TotalWarehouses
    /// </summary>
    public int TotalWarehouses { get; set; }
    /// <summary>
    /// Total Inventory locations
    /// </summary>
    public int TotalLocations { get; set; }

    /// <summary>
    /// Total Inventory pallets
    /// </summary>
    public int TotalPallets { get; set; }
    /// <summary>
    /// Warehouse Capacity = StorageSlot
    /// </summary>
    public int WarehouseCapacity { get; set; }
    /// <summary>
    /// Low Inventory Alert
    /// Ex: a low stock warning when the product quantity falls below 50 units.
    /// </summary>
    public int LowInventoryAlert { get; set; }
}

/// <summary>
/// Pending Orders
/// </summary>
public class OrderStatusDTO
{
    /// <summary>
    /// Total count of pending orders
    /// </summary>
    public int Count { get; set; }
    /// <summary>
    /// Amount
    /// </summary>
    public decimal Amount { get; set; }
}

/// <summary>
/// Inbound Order
/// </summary>
public class OrderItemDTO
{
    /// <summary>
    /// Inbound Status
    /// </summary>
    public ReceiptStatus ItemStatus { get; set; }
    /// <summary>
    /// Total count of status
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Date Inbound Order
/// </summary>
public class DateOrderItemDTO
{
    /// <summary>
    /// Inbound Status
    /// </summary>
    public DateTime Date { get; set; }
    /// <summary>
    /// Total count of status
    /// </summary>
    public int TotalCount { get; set; }
}