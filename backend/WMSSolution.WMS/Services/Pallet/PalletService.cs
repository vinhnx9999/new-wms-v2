using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Enums;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Pallet;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services
{
    /// <summary>
    /// Pallet service
    /// </summary>
    public class PalletService : BaseService<PalletEntity>, IPalletService
    {
        #region Args
        /// <summary>
        /// The DBContext
        /// </summary>
        private readonly SqlDBContext _dBContext;

        /// <summary>
        /// Localizer Service
        /// </summary>
        private readonly IStringLocalizer<MultiLanguage> _stringLocalizer;

        /// <summary>
        /// Function helper
        /// </summary>
        private readonly FunctionHelper _functionHelper;

        /// <summary>
        /// host environment
        /// </summary>
        private readonly IHostEnvironment _hostEnvironment;

        #endregion

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dBContext">The DBContext</param>
        /// <param name="stringLocalizer">Localizer</param>
        /// <param name="functionHelper">function helper</param>
        /// <param name="hostEnvironment">host environment</param>
        public PalletService(
            SqlDBContext dBContext
          , IStringLocalizer<MultiLanguage> stringLocalizer
            , FunctionHelper functionHelper,
            IHostEnvironment hostEnvironment
            )
        {
            _dBContext = dBContext;
            _stringLocalizer = stringLocalizer;
            _functionHelper = functionHelper;
            _hostEnvironment = hostEnvironment;
        }
        #endregion

        #region Api
        /// <summary>
        /// Get all records
        /// </summary>
        /// <param name="currentUser">current user</param>
        /// <returns>list of pallets</returns>
        public async Task<List<PalletViewModel>> GetAllAsync(CurrentUser currentUser)
        {
            var DbSet = _dBContext.GetDbSet<PalletEntity>(currentUser.tenant_id);
            var data = await DbSet
                .Where(t => t.PalletStatus == PalletEnumStatus.Available)
                .ToListAsync();

            return data.Adapt<List<PalletViewModel>>();
        }

        /// <summary>
        /// Automatically generate pallet code
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<CreatePalletResponse?> GenaratePalletCodeAsync(CurrentUser currentUser, CancellationToken cancellationToken)
        {
            var prefix = _hostEnvironment.IsStaging() ? "SPLT"
                       : _hostEnvironment.IsProduction() ? "PLT" : "PLT";

            var palletCode = await _functionHelper.GetFormNoAsync("pallet", prefix);

            var newPallet = new PalletEntity
            {
                PalletCode = palletCode,
                TenantId = currentUser.tenant_id,
            };


            await _dBContext.GetDbSet<PalletEntity>().AddAsync(newPallet, cancellationToken);
            var result = await _dBContext.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                return new CreatePalletResponse
                {
                    Id = newPallet.Id,
                    PalletCode = palletCode,
                };
            }

            return null;
        }

        /// <summary>
        /// Page search pallets
        /// </summary>
        /// <param name="pageSearch"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<(List<PalletPageSearchDTO> data, int total)> PageSearchPallet(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            QueryCollection queries = [];
            if (pageSearch.searchObjects.Count != 0)
            {
                pageSearch.searchObjects.ForEach(s =>
                {
                    queries.Add(s);
                });
            }

            var query = _dBContext.GetDbSet<PalletEntity>(currentUser.tenant_id)
                                  .Where(t => !t.IsFull && t.PalletStatus == PalletEnumStatus.Available);

            var expression = queries.AsExpression<PalletEntity>();

            if (expression != null)
            {
                query = query.Where(expression);
            }

            int totals = await query.CountAsync(cancellationToken);

            var list = await query
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                    .Take(pageSearch.pageSize)
                    .Select(t => new PalletPageSearchDTO
                    {
                        Id = t.Id,
                        PalletCode = t.PalletCode,
                        CreateTime = t.CreatedDate,
                        PalletStatus = (int)t.PalletStatus,
                    })
                    .ToListAsync(cancellationToken);

            return (list, totals);
        }

        #endregion
    }
}

