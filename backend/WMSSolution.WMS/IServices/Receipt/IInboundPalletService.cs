using WMSSolution.Core.JWT;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;

namespace WMSSolution.WMS.IServices.Receipt;

/// <summary>
/// Inbound pallet service interface
/// </summary>
public interface IInboundPalletService : IBaseService<InboundPallet>
{
    /// <summary>
    /// Create inbound pallet with details
    /// </summary>
    /// <param name="request">request body</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>created id and message</returns>
    Task<(int id, string message)> CreateAsync(
        CreateInboundPalletRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken);
}