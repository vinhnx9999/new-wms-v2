
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Supplier;
namespace WMSSolution.WMS.IServices
{
    /// <summary>
    /// Interface of SupplierService
    /// </summary>
    public interface ISupplierService : IBaseService<SupplierEntity>
    {
        #region Api
        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="currentUser">current user</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        Task<(List<SupplierVM> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);
        /// <summary>
        /// Get all records
        /// </summary>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<List<SupplierViewModel>> GetAllAsync(CurrentUser currentUser);
        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <param name="id">primary key</param>
        /// <returns></returns>
        Task<SupplierViewModel> GetAsync(int id);
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">current user</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns></returns>
        Task<(int id, string msg)> AddAsync(AddSupplierRequest viewModel, CurrentUser currentUser, CancellationToken cancellationToken);
        /// <summary>
        /// update a record
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UpdateAsync(int id, UpdateSupplierRequest viewModel, CurrentUser currentUser, CancellationToken cancellationToken);

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> DeleteAsync(int id);

        /// <summary>
        /// import suppliers by excel
        /// </summary>
        /// <param name="datas">excel datas</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ExcelAsync(List<SupplierExcelImportViewModel> datas, CurrentUser currentUser);
        /// <summary>
        /// Import Excel Data
        /// </summary>
        /// <param name="request"></param>
        /// <param name="currentUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ImportExcelData(List<InputSupplier> request, CurrentUser currentUser, CancellationToken cancellationToken);
        /// <summary>
        /// Get Master Supplier Data
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<IEnumerable<ChainMaster>> GetMasterData(CurrentUser currentUser);
        #endregion
    }
}

