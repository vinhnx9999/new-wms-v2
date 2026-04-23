using Wms.Theme.Web.Pages.Dashboard;
using WMSSolution.Shared.MasterData;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardInfo> GetDashboardInfoAsync();
    Task<BaseUserInfo> GetUserInfo();
    Task<MasterDataDto> LoadMasterData();
}
