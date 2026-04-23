using Microsoft.AspNetCore.Mvc;
using Wms.Theme.Web.Model.Warehouse;
using Wms.Theme.Web.Services.Warehouse;

namespace Wms.Theme.Web.Views.Shared.Warehouses;

public class _WarehouseModal(IWarehouseService warehouseService)
{
    private readonly IWarehouseService _warehouseService = warehouseService;
    public async Task<JsonResult> OnPostAddWareHouse([FromBody] AddWareHouseRequest request)
    {
        if (request is null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }
        (int? data, string? message) = await _warehouseService.AddAsync(request);

        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to add warehouse" });
    }

}
