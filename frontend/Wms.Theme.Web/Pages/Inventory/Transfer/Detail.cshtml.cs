using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.StockMove;
using Wms.Theme.Web.Services.StockMove;

namespace Wms.Theme.Web.Pages.Inventory.Transfer
{
    public class DetailModel : PageModel
    {
        private readonly IStockMoveService _stockMoveService;

        public DetailModel(IStockMoveService stockMoveService)
        {
            _stockMoveService = stockMoveService;
        }

        [BindProperty(SupportsGet = true)]
        public StockmoveViewModel StockMoveModel { get; set; }

        public async Task OnGet(int id)
        {
            StockmoveViewModel? result = await _stockMoveService.GetStockMoveByIdAsync(id);
            if (result != null)
            {
                StockMoveModel = result;
            }
            else
            {
                StockMoveModel = new StockmoveViewModel();
            }
        }

        public async Task<IActionResult> OnPostConfirm(int id)
        {
            var result = await _stockMoveService.ConfirmStockMoveAsync(id);
            if (result)
            {
                return new JsonResult(new { status = true, msg = "Xác nhận thành công" });
            }
            else
            {
                return new JsonResult(new { status = false, msg = "Xác nhận Job không thành công." });
            }
        }
    }
}
