using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices.ActionLog;

namespace WMSSolution.WMS.Controllers
{
    /// <summary>
    /// action_log controller
    /// </summary>
    [Route("actionlog")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "WMS")]
    public class Action_logController : BaseController
    {
        #region Args

        /// <summary>
        /// action_log Service
        /// </summary>
        private readonly IActionLogService _actionLogService;

        /// <summary>
        /// Localizer Service
        /// </summary>
        private readonly IStringLocalizer<MultiLanguage> _stringLocalizer;

        #endregion Args

        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="action_logService">action_log Service</param>
        /// <param name="stringLocalizer">Localizer</param>
        public Action_logController(
            IActionLogService action_logService
          , IStringLocalizer<MultiLanguage> stringLocalizer
            )
        {
            this._actionLogService = action_logService;
            this._stringLocalizer = stringLocalizer;
        }

        #endregion constructor

        #region Api

        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <returns></returns>
        [HttpPost("list")]
        public async Task<ResultModel<PageData<ActionLogViewModel>>> PageAsync(PageSearch pageSearch)
        {
            var (data, totals) = await _actionLogService.PageAsync(pageSearch, CurrentUser);

            return ResultModel<PageData<ActionLogViewModel>>.Success(new PageData<ActionLogViewModel>
            {
                Rows = data,
                Totals = totals
            });
        }

        #endregion Api
    }
}