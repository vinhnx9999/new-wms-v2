/*
 * date：2022-12-21
 * developer：NoNo
 */
 using WMSSolution.Core.Services;
 using WMSSolution.Core.Models;
 using WMSSolution.Core.JWT;
 using WMSSolution.WMS.Entities.Models;
 using WMSSolution.WMS.Entities.ViewModels;
 
 namespace WMSSolution.WMS.IServices
 {
     /// <summary>
     /// Interface of WarehouseareaService
     /// </summary>
     public interface IWarehouseareaService : IBaseService<WarehouseareaEntity>
     {
         #region Api
         /// <summary>
         /// page search
         /// </summary>
         /// <param name="pageSearch">args</param>
         /// <param name="currentUser">current user</param>
         /// <returns></returns>
         Task<(List<WarehouseareaViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser);
         /// <summary>
         /// Get all records
         /// </summary>
         /// <returns></returns>
         Task<List<WarehouseareaViewModel>> GetAllAsync(int WarehouseId, CurrentUser currentUser);
         /// <summary>
         /// Get a record by id
         /// </summary>
         /// <param name="id">primary key</param>
         /// <returns></returns>
         Task<WarehouseareaViewModel> GetAsync(int id);
         /// <summary>
         /// add a new record
         /// </summary>
         /// <param name="viewModel">viewmodel</param>
         /// <param name="currentUser">current user</param>
         /// <returns></returns>
         Task<(int id, string msg)> AddAsync(WarehouseareaViewModel viewModel, CurrentUser currentUser);
        /// <summary>
        /// update a record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UpdateAsync(WarehouseareaViewModel viewModel, CurrentUser currentUser);
 
         /// <summary>
         /// delete a record
         /// </summary>
         /// <param name="id">id</param>
         /// <returns></returns>
         Task<(bool flag, string msg)> DeleteAsync(int id);

        /// <summary>
        /// get warehouseareas of the warehouse by WarehouseId
        /// </summary>
        /// <param name="WarehouseId">warehouse's id</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<List<FormSelectItem>> GetWarehouseareaByWarehouseId(int WarehouseId, CurrentUser currentUser);
         #endregion
     }
 }
 
