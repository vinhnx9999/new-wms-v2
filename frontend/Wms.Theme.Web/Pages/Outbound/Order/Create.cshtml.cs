using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Dispatch;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Customer;
using Wms.Theme.Web.Services.Dispatch;
using Wms.Theme.Web.Services.GoodsOwner;
using Wms.Theme.Web.Services.Spu;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Warehouse;

namespace Wms.Theme.Web.Pages.Outbound.Order
{
    public class CreateModel(
        IGoodOwnerService goodOwnerService,
        IStockService stockService,
        ISpuService spuService,
        IWarehouseService warehouseService,
        IDispatchService dispatchService,
        ICustomerService customerService,
        ILogger<CreateModel> logger) : PageModel
    {
        private readonly ILogger<CreateModel> _logger = logger;
        private readonly IGoodOwnerService _goodOwnerService = goodOwnerService;
        private readonly IStockService _stockService = stockService;
        private readonly ISpuService _spuService = spuService;
        private readonly IWarehouseService _warehouseService = warehouseService;
        private readonly IDispatchService _dispatchService = dispatchService;
        private readonly ICustomerService _customerService = customerService;

        /// <summary>
        /// Username from cookie for Creator field
        /// </summary>
        public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// Current date formatted for default value
        /// </summary>
        public string CurrentDate { get; set; } = string.Empty;

        /// <summary>
        /// Dispatch No 
        /// </summary>
        public string DispatchNo { get; set; } = string.Empty;


        // TODO : remove hardcode in here and get from the server
        public async Task OnGetAsync()
        {
            Creator = Request.Cookies["user_name"] ?? User.Identity?.Name ?? "System";
            CurrentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            DispatchNo = await _dispatchService.GetNextDispatchNoAsync();
        }

        #region Master Data Loading Handlers

        /// <summary>
        /// Handler to load Goods Owner list for dropdown
        /// GET ?handler=LoadGoodsOwner
        /// </summary>
        public async Task<IActionResult> OnGetLoadGoodsOwnerAsync()
        {
            try
            {
                var result = await _goodOwnerService.GetAllGoodOwner();
                return new JsonResult(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading goods owners");
                return new JsonResult(new { data = new List<object>() });
            }
        }

        /// <summary>
        /// Handler to load Customer list for dropdown
        /// GET ?handler=LoadCustomer
        /// </summary>
        public async Task<IActionResult> OnGetLoadCustomerAsync()
        {
            try
            {
                var result = await _customerService.GetAllCustomersAsync();
                return new JsonResult(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers");
                return new JsonResult(new { data = new List<object>() });
            }
        }

        /// <summary>
        /// Handler to load Warehouse list for dropdown
        /// GET ?handler=LoadWarehouse
        /// </summary>
        public async Task<IActionResult> OnGetLoadWarehouseAsync()
        {
            try
            {
                var result = await _warehouseService.GetAllAsync();
                return new JsonResult(new { data = result?.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warehouses");
                return new JsonResult(new { data = new List<object>() });
            }
        }

        #endregion

        /// <summary>
        /// Handler to search SKU available for outbound
        /// GET ?handler=SearchSkuAvailable
        /// </summary>
        public async Task<IActionResult> OnGetSearchSkuAvailableAsync(string keyword = "", int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var request = new PageSearchRequest
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    searchObjects = new List<SearchObject>()
                };

                // Add search condition if keyword is provided
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    request.searchObjects.Add(new SearchObject
                    {
                        Name = "SkuName",
                        Operator = Operators.Contains,
                        Value = keyword,
                        Text = keyword
                    });
                }

                var result = await _stockService.GetSkuAvailableAsync(request);

                if (result.IsSuccess && result.Data != null)
                {
                    return new JsonResult(new
                    {
                        success = true,
                        data = result.Data.Rows,
                        total = result.Data.Totals
                    });
                }

                return new JsonResult(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Failed to load SKU data",
                    data = new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching SKU available");
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message,
                    data = new List<object>()
                });
            }
        }

        /// <summary>
        /// Handler to get available pallets for outbound (FEFO suggestion)
        /// GET ?handler=AvailablePallets
        /// </summary>
        public async Task<IActionResult> OnGetAvailablePalletsAsync(int skuId, decimal requiredQty)
        {
            try
            {
                var result = await _stockService.GetAvailableForOutboundAsync(skuId, requiredQty);

                if (result.IsSuccess && result.Data != null)
                {
                    return new JsonResult(new
                    {
                        success = true,
                        data = result.Data
                    });
                }

                return new JsonResult(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Failed to load available pallets",
                    data = new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available pallets for SKU {SkuId}", skuId);
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message,
                    data = new List<object>()
                });
            }
        }

        /// <summary>
        /// Save as Draft - creates dispatch with Draft status (no stock lock)
        /// POST ?handler=Draft
        /// </summary>
        public async Task<IActionResult> OnPostDraftAsync([FromBody] CreateOutboundOrderRequest request)
        {
            _logger.LogInformation("Create Outbound Draft request received: dispatch_no={DispatchNo}, customer={Customer}, items={ItemCount}",
                request?.dispatch_no, request?.customer_name, request?.detailList?.Count ?? 0);

            if (request == null)
            {
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(request.dispatch_no))
            {
                return new JsonResult(new { success = false, message = "Vui lòng nhập Số lệnh xuất kho." });
            }

            if (request.detailList == null || request.detailList.Count == 0)
            {
                return new JsonResult(new { success = false, message = "Vui lòng thêm ít nhất một dòng hàng hóa." });
            }

            try
            {
                var draftRequest = MapToDispatchDraftRequest(request);
                var result = await _dispatchService.CreateDraftAsync(draftRequest);

                if (result.IsSuccess)
                {
                    string redirectUrl = Url.Page("Index") ?? "/Outbound/Order";
                    return new JsonResult(new
                    {
                        success = true,
                        message = "Lưu tạm lệnh xuất kho thành công!",
                        redirectUrl
                    });
                }

                return new JsonResult(new
                {
                    success = false,
                    message = result.Message ?? "Lưu tạm lệnh xuất kho thất bại."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating draft outbound order");
                return new JsonResult(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Submit - creates and executes dispatch in one step (locks stock)
        /// POST ?handler=Submit
        /// </summary>
        public async Task<IActionResult> OnPostSubmitAsync([FromBody] CreateOutboundOrderRequest request)
        {
            _logger.LogInformation("Create Outbound Submit request received: dispatch_no={DispatchNo}, customer={Customer}, items={ItemCount}",
                request?.dispatch_no, request?.customer_name, request?.detailList?.Count ?? 0);

            if (request == null)
            {
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(request.dispatch_no))
            {
                return new JsonResult(new { success = false, message = "Vui lòng nhập Số lệnh xuất kho." });
            }

            if (request.detailList == null || request.detailList.Count == 0)
            {
                return new JsonResult(new { success = false, message = "Vui lòng thêm ít nhất một dòng hàng hóa." });
            }

            try
            {
                var draftRequest = MapToDispatchDraftRequest(request);
                var result = await _dispatchService.CreateAndExecuteAsync(draftRequest);

                if (result.IsSuccess)
                {
                    string redirectUrl = Url.Page("Index") ?? "/Outbound/Order";
                    return new JsonResult(new
                    {
                        success = true,
                        message = "Tạo lệnh xuất kho thành công!",
                        redirectUrl
                    });
                }

                return new JsonResult(new
                {
                    success = false,
                    message = result.Message ?? "Tạo lệnh xuất kho thất bại."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting outbound order");
                return new JsonResult(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        #region Private Helpers

        /// <summary>
        /// Map frontend request DTO to the DispatchDraftRequest used by DispatchService
        /// </summary>
        private static DispatchDraftRequest MapToDispatchDraftRequest(CreateOutboundOrderRequest request)
        {
            return new DispatchDraftRequest
            {
                CustomerId = request.customer_id,
                CustomerName = request.customer_name ?? string.Empty,
                Items = request.detailList?.Select(d => new DispatchDraftItem
                {
                    SkuId = d.sku_id,
                    Qty = d.dispatch_qty_decimal > 0 ? d.dispatch_qty_decimal : d.dispatch_qty,
                    PalletSelections = d.selected_locations?.Select(loc => new PalletSelection
                    {
                        PalletId = loc.pallet_id,
                        LocationId = loc.location_id,
                        PickQty = loc.pick_qty
                    }).ToList() ?? new List<PalletSelection>()
                }).ToList() ?? new List<DispatchDraftItem>()
            };
        }

        #endregion
    }
}
