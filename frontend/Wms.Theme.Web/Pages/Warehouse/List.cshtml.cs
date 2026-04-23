using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Warehouse;
using Wms.Theme.Web.Services.Warehouse;
namespace Wms.Theme.Web.Pages.Warehouse;
public class ListModel(IWarehouseService service) : PageModel
{
    private readonly IWarehouseService _service = service;

    public List<WarehouseInfoDTO> Warehouses { get; set; } = [];

    public async Task OnGetAsync()
    {
        var data = await _service.GetAllAsync();       
        if (data != null && data.IsSuccess)
        {
            var items = data.Data
                .OrderByDescending(x => x.is_valid)
                .ThenBy(x => x.WarehouseName);
            Warehouses = Convert2Models(items);
        }
    }

    public async Task<JsonResult> OnPostActiveWareHouse([FromBody] BasePostActionRequest request)
    {
        if (request.Id <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        (int? data, string? message) = await _service.ActiveWareHouse(request.Id);
        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to active warehouse" });
    }

    public async Task<JsonResult> OnPostDeActiveWareHouse([FromBody] BasePostActionRequest request)
    {
        if (request.Id <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        (int? data, string? message) = await _service.DeActiveWareHouse(request.Id);
        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to de-active warehouse" });
    }

    public async Task<JsonResult> OnPostAddWareHouse([FromBody] AddWareHouseRequest request)
    {
        if (request is null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }
        (int? data, string? message) = await _service.AddAsync(request);

        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to add warehouse" });
    }    

    private List<WarehouseInfoDTO> Convert2Models(IEnumerable<WarehouseViewModel> data)
    {
        if (!data.Any()) return [];

        return [.. data.Select(d => new WarehouseInfoDTO
        {
            Id = d.id,
            Code = $"WH-{d.id:000}",
            Name = d.WarehouseName,
            Address = $"{d.address}",
            City = $"{d.city}",
            TotalPallet = d.TotalPallet,
            LocationCount = d.LocationCount,
            TotalInventory = d.TotalInventory ,
            Invalid = !d.is_valid,
            WcsBlockId = d.WcsBlockId
        })];
    }
}

public class WarehouseInfoDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public int LocationCount { get; set; }
    public int TotalInventory { get; set; }
    public int TotalPallet { get; set; }
    public bool Invalid { get; set; } = false;
    public string? WcsBlockId { get; set; }
}
