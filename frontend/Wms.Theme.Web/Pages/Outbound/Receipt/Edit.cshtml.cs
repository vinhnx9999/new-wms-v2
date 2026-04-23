using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Customer;
using Wms.Theme.Web.Model.OutboundGateway;
using Wms.Theme.Web.Model.OutboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Stock;
using Wms.Theme.Web.Services.Customer;
using Wms.Theme.Web.Services.OutboundGateway;
using Wms.Theme.Web.Services.Pallet;
using Wms.Theme.Web.Services.Receipt;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.Warehouse;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Pages.Outbound.Receipt
{
    public class EditModel(IWarehouseService warehouseService,
                             IOutboundReceiptService outboundReceiptService,
                             ICustomerService customerService,
                             IOutboundGatewayService outboundGatewayService,
                             ISkuService skuService,
                             IStockService stockService,
                             IPalletService palletService) : PageModel
    {
        private readonly IOutboundReceiptService _outboundReceiptService = outboundReceiptService;
        private readonly IWarehouseService _warehouseService = warehouseService;
        private readonly ICustomerService _customerService = customerService;
        private readonly IOutboundGatewayService _outboundGatewayService = outboundGatewayService;
        private readonly ISkuService _skuService = skuService;
        private readonly IStockService _stockService = stockService;
        private readonly IPalletService _palletService = palletService;

        /// <summary>
        /// constant for page index
        /// </summary>
        private const int PAGE_INDEX = 1;

        [BindProperty]
        public OutboundReceiptDetailedDto Receipt { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var dto = await _outboundReceiptService.GetReceiptDetailAsync(id);
            if (dto == null || dto.Id <= 0)
            {
                return NotFound();
            }
            if (dto.Status >= 1 && !dto.Details.Any(x => x.IsException))
            {
                TempData["ErrorMessage"] = "Phiếu này đã được xử lý, không thể chỉnh sửa!";
                return RedirectToPage("/Inbound/Receipt/Detail", new { id = id });
            }
            Receipt = dto;
            return Page();
        }

        public async Task<JsonResult> OnGetSearchWareHouse(string? keyWord, int pageIndex = PAGE_INDEX)
        {
            var searches = new List<SearchObject>
                {
                    new() {
                        Name = "WarehouseName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "WarehouseName"
                    }
                };

            PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
            var data = await _warehouseService.PageSearchWarehouse(pageSearch);
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnGetSearchSku(string? keyWord, int pageIndex = PAGE_INDEX, int? warehouseId = null)
        {
            var searches = new List<SearchObject>
                {
                    new() {
                        Name = "SkuName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "SkuName",
                        Group = "Search"
                    },
                    new() {
                        Name = "SkuCode",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "SkuCode",
                        Group = "Search"
                    },
                    new() {
                        Name = "SupplierName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "SupplierName",
                        Group = "Search"
                    },
                    new() {
                        Name = "UnitName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "UnitName",
                        Group = "Search"
                    }
            };

            PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
            var data = await _skuService.PageSearch(pageSearch, warehouseId);
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnGetSearchCustomer(string? keyWord, int pageIndex = PAGE_INDEX)
        {
            var searches = new List<SearchObject>                
            {
                    new() {
                        Name = "CustomerName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "CustomerName"
                    }
            };

            PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
            var data = await _customerService.PageSearchAsync(pageSearch);
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnGetSearchOutboundGateway(string? keyWord, int warehouseId, int pageIndex = PAGE_INDEX)
        {
            var searches = new List<SearchObject>
            {
                new() {
                        Name = "GatewayName",
                        Value = keyWord ?? "",
                        Text = keyWord ?? "",
                        Operator = Operators.Contains,
                        Label = "GatewayName"
                    }
            };

            PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
            var data = await _outboundGatewayService.PageSearchByWarehouse(pageSearch, warehouseId);
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnPostUpdateReceipt([FromRoute] int id, [FromBody] UpdateOutboundReceiptRequest request)
        {
            if (request is null)
            {
                return new JsonResult(new { success = false, message = "Invalid request data" });
            }

            var result = await _outboundReceiptService.UpdateReceiptAsync(id, request);
            if (!result)
            {
                return new JsonResult(new { success = false, message = "Failed to update receipt" });
            }
            return new JsonResult(new { success = true });
        }

        /// <summary>
        /// Page Search for location with pallet
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnGetSearchLocationWithPallet(int warehouseId, int skuId)
        {
            var request = new GetLocationStockBySkuRequest
            {
                SkuId = skuId,
                WarehouseId = warehouseId
            };

            var data = await _stockService.FilterSkuLocationStock(request);
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnGetSearchPallet(string? keyword, int pageIndex = PAGE_INDEX)
        {
            List<SearchObject> searches = [new SearchObject
                    {
                        Name = "PalletCode",
                        Value = keyword ?? "",
                        Text = keyword ?? "",
                        Operator = Operators.Contains,
                        Label = "PalletCode",
                    },

                    new SearchObject
                    {
                        Name = "PalletStatus",
                        Type = "number",
                        Operator = Operators.Equal,
                        Text = "1",
                        Value = 1,
                        Label = "PalletStatus",
                    }];

            PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
            var data = await _palletService.SearchAsync(pageSearch);
            return new JsonResult(data);
        }

        /// <summary>
        /// Generate pallet code
        /// </summary>
        /// <returns>New Pallet</returns>
        public async Task<JsonResult> OnGetGeneratePalletCode()
        {
            var result = await _palletService.GetNextPalletCode();
            return new JsonResult(result);
        }
        
        public async Task<JsonResult> OnGetGenarationReceiptNo()
        {
            var result = await _outboundReceiptService.GetNextReceiptCode();
            return new JsonResult(result);
        }

        /// <summary>
        /// Add outbound gateway 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnPostQuickOutboundGateway([FromBody] AddOutboundGatewayRequest request)
        {
            if (request is null)
            {
                return new JsonResult(new { success = false, message = "Invalid request data" });
            }

            if (string.IsNullOrWhiteSpace(request.GatewayName))
            {
                return new JsonResult(new { success = false, message = "Gateway name is required" });
            }

            if (request.WarehouseId <= 0)
            {
                return new JsonResult(new { success = false, message = "Warehouse is required" });
            }

            var (id, message) = await _outboundGatewayService.AddAsync(request);
            if (id == 0)
            {
                return new JsonResult(new { success = false, message });
            }

            return new JsonResult(new
            {
                success = true,
                id,
                gatewayName = request.GatewayName,
                message
            });
        }

        public async Task<JsonResult> OnPostAddCustomer([FromBody] AddCustomerRequest request)
        {
            if (request is null)
            {
                return new JsonResult(new { success = false, message = "Invalid request data" });
            }

            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                return new JsonResult(new { success = false, message = "Customer name is required" });
            }

            var (id, message) = await _customerService.AddAsync(request);
            if (id == null)
            {
                return new JsonResult(new { success = false, message });
            }

            return new JsonResult(new
            {
                success = true,
                id,
                customerName = request.CustomerName,
                message
            });
        }
    }
}

