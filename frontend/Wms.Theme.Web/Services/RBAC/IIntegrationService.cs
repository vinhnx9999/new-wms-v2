using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Services.RBAC;

public interface IIntegrationService
{
    Task<IntegrationInfo> GetIntegrationInfo();
    Task<(bool isSuccess, string? message)> UpdateIntegrationInfo(IntegrationInfo request);
}
