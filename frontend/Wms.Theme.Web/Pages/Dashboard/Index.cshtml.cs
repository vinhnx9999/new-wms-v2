using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Domain.Enums;
using Wms.Theme.Web.Services.Dashboard;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Pages.Dashboard;

public class IndexModel(IDashboardService service) : PageModel
{
    private readonly IDashboardService _service = service;    
    public DashboardInfo Info { get; set; } = new();
    public BaseUserInfo UserInfo { get; set; } = new();   

    public async Task OnGetAsync()
    {
        await TryGetUserInfoAsync();
        var info = await _service.GetDashboardInfoAsync();
        Info = info;
        ViewData["Title"] = "Dashboard";
    }

    public async Task<JsonResult> OnGetLoadMasterData()
    {
        var data = await _service.LoadMasterData();
        return new JsonResult(data);
    }

    private async Task TryGetUserInfoAsync()
    {
        try
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (string.IsNullOrEmpty(role))
            {
                var userInfo = await _service.GetUserInfo();
                UserInfo = userInfo;

                TempData["DisplayName"] = userInfo.DisplayName;
                TempData["UserName"] = userInfo.Username;
                TempData["UserRole"] = userInfo.Role;

                TempData.Keep("UserName"); // keep it alive
                TempData.Keep("UserRole"); // keep it alive
                TempData.Keep("DisplayName");
            }
        }
        catch
        {

        }
    }
} 

public class DashboardInfo
{
    public int LowInventoryAlert { get; set; } = 0;
    public int TotalWarehouses { get; set; } = 0;
    public int TotalItems { get; set; } = 0;
    public int TotalInventory { get; set; } = 0;
    public int TotalPallets { get; set; } = 0;
    public int WarehouseCapacity { get; set; } = 0;
    public int PendingOrders { get; set; } = 0;
    public int ProcessingOrders { get; set; } = 0;
    public int TodayOrders { get; set; } = 0;
    public int YesterdayOrders { get; set; } = 0;
    public IEnumerable<DateOrderItemDTO> InboundItems { get; set; } = [];
    public IEnumerable<OrderItemDTO> Items { get; set; } = [];
    public IEnumerable<DateOrderItemDTO> OutboundItems { get; set; } = [];
    public int CapacityPercent
    {
        get
        {
            return TotalInventory == 0 ? 0 :
                (int)Math.Round((decimal)WarehouseCapacity / TotalInventory * 100, 2);
        }
    }

    public int PercentIncrease
    {
        get
        {
            return YesterdayOrders == 0 ? 0 :
                (int)Math.Round((decimal)(TodayOrders - YesterdayOrders) / YesterdayOrders * 100, 2);
        }
    }

    public IEnumerable<string> Inbound7Dates
    {
        get
        {
            var last7Days = new List<string>();
            var today = DateTime.Now.AddDays(-7);

            for (int i = 0; i < 7; i++)
            {
                last7Days.Add(today.AddDays(i).ToString("yyyy-MM-dd"));
            }

            return last7Days;
        }
    }

    public IEnumerable<int> Inbound7DateValues
    {
        get
        {
            var last7Days = new List<int>();
            var today = DateTime.Now.AddDays(-7);

            for (int i = 0; i < 7; i++)
            {
                var item = InboundItems.FirstOrDefault(x => x.Date.Date == today.AddDays(i).Date);
                last7Days.Add(item?.TotalCount ?? 0);
            }

            return last7Days;
        }
    }
}

public class PendingOrdersDTO
{
    public int Count { get; set; }
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