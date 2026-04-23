using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Services.AsnMaster;

namespace Wms.Theme.Web.Pages.Inbound.ASN;

public class DetailModel(IAsnMasterService asnMasterService) : PageModel
{
    private readonly IAsnMasterService _asnMasterService = asnMasterService;

    [BindProperty]
    public AsnMasterCustomDetailedDTO AsnMasterDetail { get; set; } = new AsnMasterCustomDetailedDTO();

    public async Task OnGetAsync(int id)
    {
        AsnMasterDetail = await _asnMasterService.GetAnsMasterDetailed(id) ?? new AsnMasterCustomDetailedDTO();
    }
}
