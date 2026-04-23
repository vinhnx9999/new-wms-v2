using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Inbound;
using WMSSolution.WMS.Entities.Models.OutboundGateway;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.ViewModels.Dashboard;
using WMSSolution.WMS.Entities.ViewModels.OutboundGateway;
using WMSSolution.WMS.IServices.OutboundGateway;

namespace WMSSolution.WMS.Services.OutboundGateway
{
    /// <summary>
    /// implementation of IOutboundGatewayService
    /// </summary>
    public class OutboundGatewayService(SqlDBContext dBContext,
        ILogger<OutboundGatewayService> logger) : IOutboundGatewayService
    {
        private readonly SqlDBContext _dBContext = dBContext;
        private readonly ILogger<OutboundGatewayService> _logger = logger;

        /// <summary>
        /// Adding OutboundGateway
        /// </summary>
        /// <param name="request"></param>
        /// <param name="currentUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(int id, string message)> AddAsync(AddOutboundGatewayRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                _logger.LogInformation("AddOutboundGatewayRequest is null with CurrentUser: {CurrentUser}", currentUser);
                return (0, "Request is empty");
            }
            var entity = new OutboundGatewayEntity
            {
                GatewayName = request.GatewayName,
                WarehouseId = request.WarehouseId,
                CreateTime = DateTime.UtcNow
            };
            await _dBContext.AddAsync(entity, cancellationToken);
            var result = await _dBContext.SaveChangesAsync(cancellationToken);
            if (result <= 0)
            {
                _logger.LogError("Failed to add OutboundGateway with Request: {Request} and CurrentUser: {CurrentUser}", request, currentUser);
                return (0, "Failed to add Outbound Gateway");
            }
            return (entity.Id, "Outbound Gateway added successfully");
        }

        /// <summary>
        /// Page search Request
        /// </summary>
        /// <param name="pageSearch"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="currentUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<(List<OutboundGatewayResponse> data, int total)> PageAsyncByWarehouseId(PageSearch pageSearch, int wareHouseId, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            QueryCollection queries = [];
            if (pageSearch.searchObjects.Count != 0)
            {
                pageSearch.searchObjects.ForEach(s =>
                {
                    queries.Add(s);
                });
            }

            var query = _dBContext.GetDbSet<OutboundGatewayEntity>(currentUser.tenant_id)
                                  .Where(x => x.WarehouseId == wareHouseId);

            var expression = queries.AsExpression<OutboundGatewayEntity>();
            if (expression is not null)
            {
                query = query.Where(expression);
            }
            int total = await query.CountAsync(cancellationToken);
            if (total == 0)
            {
                _logger.LogInformation("No OutboundGateway data found for WarehouseId: {WarehouseId}", wareHouseId);
                return ([], total);
            }
            var data = await query.OrderByDescending(x => x.CreateTime)
                                  .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                                  .Take(pageSearch.pageSize)
                                  .Select(x => new OutboundGatewayResponse
                                  {
                                      Id = x.Id,
                                      GatewayName = x.GatewayName,
                                      CreateTime = x.CreateTime
                                  }).ToListAsync(cancellationToken);
            return (data, total);
        }

        /// <summary>
        /// Get Outbound By RangeDate
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="finishStatuses"></param>
        /// <param name="fromDate"></param>
        /// <param name="today"></param>
        /// <returns></returns>
        public async Task<IEnumerable<DateOrderItemDTO>> GetOutboundByRangeDate(CurrentUser currentUser,
            ReceiptStatus[] finishStatuses, DateTime fromDate, DateTime today)
        {
            return [];
            //var query = from receipt in _dBContext.GetDbSet<OutboundGatewayEntity>(currentUser.tenant_id)
            //            where //finishStatuses.Contains(receipt.) &&
            //                receipt.LastUpdateTime.Date >= fromDate.Date
            //                && receipt.LastUpdateTime.Date <= today.Date
            //            //group receipt by receipt.Status into g
            //            select new OrderItemDTO
            //            {
            //                ItemStatus = g.Key,
            //                TotalCount = g.Count()
            //            };

            //return query.ToList();
        }
    }
}
