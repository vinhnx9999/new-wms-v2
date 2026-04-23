using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Wms.Theme.Web.Pages.System;

public class IndexModel : PageModel
{
    public IndexModel()
    {
    }

    public SystemInfoDTO SystemInfo { get; set; } = new();

    public async Task OnGetAsync()
    {
        await Task.Yield(); // Make it truly async
        SystemInfo.DatabaseVersion = "SQLite 3.0 (Mock)";
        SystemInfo.ApplicationVersion = "1.0.0";
        SystemInfo.NetVersion = ".NET 9.0.0";
        SystemInfo.LastUpdated = DateTime.UtcNow;

        // Get some statistics (Mock)
        SystemInfo.TotalWarehouses = 5;
        SystemInfo.TotalLocations = 150;
        SystemInfo.TotalItems = 1200;
        SystemInfo.TotalOrders = 350;
    }
}

public class SystemInfoDTO
{
    public string DatabaseVersion { get; set; } = "";
    public string ApplicationVersion { get; set; } = "";
    public string NetVersion { get; set; } = "";
    public DateTime LastUpdated { get; set; }
    public int TotalWarehouses { get; set; }
    public int TotalLocations { get; set; }
    public int TotalItems { get; set; }
    public int TotalOrders { get; set; }
}
