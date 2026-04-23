using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Supplier;
using Wms.Theme.Web.Services.Supplier;

namespace Wms.Theme.Web.Pages.Setting.Supplier
{
    public class CreateModel(ISupplierService supplierService) : PageModel
    {
        private readonly ISupplierService _supplierService = supplierService;

        [BindProperty(SupportsGet = true)]
        public AddSupplierRequest AddSupplierRequest { get; set; } = default!;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            var result = await _supplierService.AddSuplierAsync(AddSupplierRequest);
            return RedirectToPage("./Index");
        }
    }
}
