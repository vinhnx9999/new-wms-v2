using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;

namespace WMSSolution.WMS.IServices.ActionLog;

/// <summary>
/// Interface of Action_logService
/// </summary>
public interface IActionLogService : IBaseService<ActionLogEntity>
{
    #region Api

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(List<ActionLogViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser);

    /// <summary>
    /// add a new log record
    /// </summary>
    /// <returns></returns>
    Task<bool> AddLogAsync(string content, string actionName, CurrentUser currentUser);

    #endregion Api
}