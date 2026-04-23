using Wms.Theme.Web.Model.InboundReceipt;

namespace Wms.Theme.Web.Services.InboundPallet;

public interface IInboundPalletService
{
    Task<(int id, string message)> CreateAsync(CreateInboundPalletRequest request, CancellationToken cancellationToken);
}