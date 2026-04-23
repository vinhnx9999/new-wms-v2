using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Dispatch;
using Wms.Theme.Web.Services.Dispatch;

namespace Wms.Theme.Web.Pages.Outbound.SalesOrders
{
    public class DetailModel : PageModel
    {

        private readonly IDispatchService _dispatchService;

        public DetailModel(IDispatchService dispatchService)
        {
            _dispatchService = dispatchService;
        }

        [BindProperty]
        public List<DispatchDetailDTO> Detail { get; set; } = new List<DispatchDetailDTO>();

        public async Task<IActionResult> OnGet(string id)
        {
            Detail = await _dispatchService.GetByDispatchlistNo(id) ?? new List<DispatchDetailDTO>();
            return Page();
        }
    }
}
