using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Planning;
using Wms.Theme.Web.Services.Planning;

namespace Wms.Theme.Web.Pages.Planning.Packing
{
    public class IndexModel(IPlanningService service) : PageModel
    {
        private readonly IPlanningService _service = service;
        public IEnumerable<PickingDTO> PickingList { get; set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var pickings = await _service.GetPackingList();
            PickingList = pickings;
            return Page();
        }
    }
}
