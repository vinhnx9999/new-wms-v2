
using Microsoft.EntityFrameworkCore;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices.ActionLog;

namespace WMSSolution.WMS.Services.ActionLog;

/// <summary>
///  ActionLog Service
/// </summary>
/// <remarks>
///ActionLog  constructor
/// </remarks>
/// <param name="dbContext">The DBContext</param>
public class ActionLogService(SqlDBContext dbContext) : BaseService<ActionLogEntity>, IActionLogService
{
    #region Args

    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dbContext = dbContext;

    #endregion Args

    #region Api

    /// <summary>
    /// add a new log record
    /// </summary>
    /// <returns></returns>
    public async Task<bool> AddLogAsync(string content, string actionName, CurrentUser currentUser)
    {
        var DbSet = _dbContext.GetDbSet<ActionLogEntity>();
        var entity = new ActionLogEntity
        {
            action_content = content,
            Id = 0,
            action_time = DateTime.UtcNow,
            action_name = actionName,
            TenantId = currentUser.tenant_id,
            user_name = currentUser.user_name
        };

        try
        {
            await DbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id > 0;
        }        
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(List<ActionLogViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Any())
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }
        var query = from log in _dbContext.GetDbSet<ActionLogEntity>().AsNoTracking()
                    where log.TenantId == currentUser.tenant_id
                    select new ActionLogViewModel
                    {
                        id = log.Id,
                        user_name = log.user_name,
                        action_content = log.action_content,
                        action_time = log.action_time,
                    };
        query = query.Where(queries.AsExpression<ActionLogViewModel>());
        int totals = await query.CountAsync();
        var list = await query.OrderByDescending(t => t.action_time)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync();
        return (list, totals);
    }

    #endregion Api
}