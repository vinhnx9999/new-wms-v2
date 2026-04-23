using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Services.Asn;
namespace Wms.Theme.Web.Pages.Inbound.Process;
public class DetailModel(IAsnService asnService) : PageModel
{
    private readonly IAsnService _asnService = asnService;

    public AsnDto AsnModel { get; set; } = new AsnDto();

    public async Task OnGet(int id)
    {
        var model = await _asnService.GetAsnDetailsAsync(id);
        if (model == null)
        {
            TempData["ErrorMessage"] = "Asn details loaded fail";
            return;
        }
        AsnModel = model.Data!;
        TempData["SuccessMessage"] = "ASN details loaded successfully.";
    }
}
