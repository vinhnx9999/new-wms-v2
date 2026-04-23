using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Planning;
using Wms.Theme.Web.Services.Planning;

namespace Wms.Theme.Web.Pages.Planning.Picking;

public class IndexModel(IPlanningService service) : PageModel
{
    private readonly IPlanningService _service = service;
    public IEnumerable<PickingDTO> PickingList { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        var pickings = await _service.GetPickingList();
        PickingList = pickings;
        return Page();
    }

    public async Task<JsonResult> OnPostAddPicking([FromBody] AddPickingPlanningDTO request)
    { 
        if (request is null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        var (Success, Message) = await _service.SavePickingList(request.PickingList);
        if (!Success)
        {
            return new JsonResult(new { success = false, message = Message  ?? "Failed to Save Picking" });
        }
        return new JsonResult(new { success = true });
    }
}
