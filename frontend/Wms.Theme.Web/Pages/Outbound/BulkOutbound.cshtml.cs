using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.Customer;
using Wms.Theme.Web.Model.GoodLocation;
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

namespace Wms.Theme.Web.Pages.Outbound
{
    public class BulkOutboundModel(IWarehouseService warehouseService,
                         IOutboundReceiptService outboundReceiptService,
                         ICustomerService customerService,
                         IOutboundGatewayService outboundGatewayService,
                         ISkuService skuService,
                         IStockService stockService,
                         IPalletService palletService) : PageModel
    {
        private readonly IWarehouseService _warehouseService = warehouseService;
        private readonly IOutboundReceiptService _outboundReceiptService = outboundReceiptService;
        private readonly ICustomerService _customerService = customerService;
        private readonly IOutboundGatewayService _outboundGatewayService = outboundGatewayService;
        private readonly ISkuService _skuService = skuService;
        private readonly IStockService _stockService = stockService;
        private readonly IPalletService _palletService = palletService;

        [BindProperty(SupportsGet = true)]
        public IEnumerable<GoodSkuLocationInfo> SkuLocations { get; set; } = [];

        /// <summary>
        /// constant for page index
        /// </summary>
        private const int PAGE_INDEX = 1;

        public void OnGet()
        {
        }

        public async Task<JsonResult> OnGetSearchWareHouse(string? keyWord, int pageIndex = PAGE_INDEX)
        {
            List<SearchObject> searches = [
                    new SearchObject
                {
                    Name = "WarehouseName",
                    Value = keyWord ?? "",
                    Text = keyWord ?? "",
                    Operator = Operators.Contains,
                    Label = "WarehouseName"
                }
            ];

            PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
            var data = await _warehouseService.PageSearchWarehouse(pageSearch);
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnGetSearchSku(string? keyWord, int pageIndex = PAGE_INDEX, int pageSize = SystemConfig.PAGE_SIZE, int? warehouseId = null)
        {
            List<SearchObject> searches =
                [
                    new SearchObject
                {
                    Name = "SkuName",
                    Value = keyWord ?? "",
                    Text = keyWord ?? "",
                    Operator = Operators.Contains,
                    Label = "SkuName",
                    Group = "Search"
                },
                new SearchObject
                {
                    Name = "SkuCode",
                    Value = keyWord ?? "",
                    Text = keyWord ?? "",
                    Operator = Operators.Contains,
                    Label = "SkuCode",
                    Group = "Search"
                },
                new SearchObject
                {
                    Name = "SupplierName",
                    Value = keyWord ?? "",
                    Text = keyWord ?? "",
                    Operator = Operators.Contains,
                    Label = "SupplierName",
                    Group = "Search"
                },
                new SearchObject
                {
                    Name = "UnitName",
                    Value = keyWord ?? "",
                    Text = keyWord ?? "",
                    Operator = Operators.Contains,
                    Label = "UnitName",
                    Group = "Search"
                }
            ];

            PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
            var data = await _skuService.PageSearch(pageSearch, warehouseId);
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnGetSearchCustomer(string? keyWord, int pageIndex = PAGE_INDEX)
        {
            List<SearchObject> searches =
                [
                    new SearchObject
                {
                    Name = "CustomerName",
                    Value = keyWord ?? "",
                    Text = keyWord ?? "",
                    Operator = Operators.Contains,
                    Label = "CustomerName"
                }
            ];
            PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
            var data = await _customerService.PageSearchAsync(pageSearch);
            return new JsonResult(data);
        }


        public async Task<JsonResult> OnGetSearchOutboundGateway(string? keyWord, int warehouseId, int pageIndex = PAGE_INDEX)
        {
            List<SearchObject> searches =
                [
                    new SearchObject
                {
                    Name = "GatewayName",
                    Value = keyWord ?? "",
                    Text = keyWord ?? "",
                    Operator = Operators.Contains,
                    Label = "GatewayName"
                }
            ];

            PageSearchRequest pageSearch = SearchUtil.GetPageSearch(searches, pageIndex);
            var data = await _outboundGatewayService.PageSearchByWarehouse(pageSearch, warehouseId);
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnPost([FromBody] CreateOutboundReceiptRequest request)
        {
            if (request is null)
            {
                return new JsonResult(new { success = false, message = "Invalid request data" });
            }
            var (id, message) = await _outboundReceiptService.CreateOutboundReceiptAsync(request);
            if (id == 0)
            {
                return new JsonResult(new { success = false, message });
            }
            return new JsonResult(new { success = true, id });
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
            SkuLocations = data;
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnGetSearchPallet(string? keyword, int pageIndex = PAGE_INDEX)
        {
            List<SearchObject> searches =
                [
                    new SearchObject
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
                }
                ];

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

