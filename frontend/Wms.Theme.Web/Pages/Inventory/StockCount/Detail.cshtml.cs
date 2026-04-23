using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.StockTaking;
using Wms.Theme.Web.Services.StockTaking;

namespace Wms.Theme.Web.Pages.Inventory.StockCount
{
    public class DetailModel : PageModel
    {
        private readonly IStockTakingService _stockTakingService;

        public DetailModel(IStockTakingService stockTakingService)
        {
            _stockTakingService = stockTakingService;
        }

        [BindProperty(SupportsGet = true)]
        public StocktakingViewModel Stocktaking { get; set; }

        public async Task OnGet(int id)
        {
            StocktakingViewModel? result = await _stockTakingService.GetStockTakingById(id);
            if (result != null)
            {
                Stocktaking = result;
            }
            else
            {
                Stocktaking = new StocktakingViewModel();
            }
        }
    }
}
