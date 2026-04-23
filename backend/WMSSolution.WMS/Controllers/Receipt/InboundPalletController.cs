using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;
using WMSSolution.WMS.IServices.Receipt;

namespace WMSSolution.WMS.Controllers.Receipt;

/// <summary>
/// inbound pallet Controller 
/// </summary>
/// <param name="service"></param>
[Route("inbound-pallet")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class InboundPalletController(IInboundPalletService service) : BaseController
{
    /// <summary>
    /// Create inbound method 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("create")]
    public async Task<ResultModel<int>> CreateAsync([FromBody] CreateInboundPalletRequest request, CancellationToken cancellationToken)
    {
        var (id, message) = await service.CreateAsync(request, CurrentUser, cancellationToken);
        return id > 0 ? ResultModel<int>.Success(id, message) : ResultModel<int>.Error(message);
    }
}