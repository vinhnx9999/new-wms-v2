using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.ViewModels.OutboundGateway;
using WMSSolution.WMS.IServices.OutboundGateway;

namespace WMSSolution.WMS.Controllers.OutboundGateway
{
    /// <summary>
    /// Outbound Gateway controller
    /// </summary>
    [Route("outbound-gateway")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "WMS")]
    public class OutboundGatewayController(IOutboundGatewayService outboundGatewayService) : BaseController
    {
        private readonly IOutboundGatewayService _outboundGatewayService = outboundGatewayService;

        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="cancellation">cancellation token</param>
        /// <param name="wareHouseId">warehouse ID</param>
        /// <returns>list of gateway</returns>
        [HttpPost("list-by-warehouse")]
        public async Task<ResultModel<PageData<OutboundGatewayResponse>>> PageAsync([FromBody] PageSearch pageSearch, [FromQuery] int wareHouseId, CancellationToken cancellation)
        {
            var (data, totals) = await _outboundGatewayService.PageAsyncByWarehouseId(pageSearch, wareHouseId, CurrentUser, cancellation);

            return ResultModel<PageData<OutboundGatewayResponse>>.Success(new PageData<OutboundGatewayResponse>
            {
                Rows = data,
                Totals = totals
            });
        }

        /// <summary>
        /// Add a new outbound gateway
        /// </summary>
        /// <param name="request">The request containing the outbound gateway details</param>
        /// <param name="cancellation">The cancellation token</param>
        /// <returns>The result of the add operation</returns>
        [HttpPost]
        public async Task<ResultModel<int>> AddAsync([FromBody] AddOutboundGatewayRequest request, CancellationToken cancellation)
        {
            var (id, message) = await _outboundGatewayService.AddAsync(request, CurrentUser, cancellation);
            if (id > 0)
            {
                return ResultModel<int>.Success(id);
            }
            else
            {
                return ResultModel<int>.Error(message);
            }
        }

    }
}
