using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Wms.Theme.Domain.Entities;
using Wms.Theme.Web.Model.Warehouse;

namespace Wms.Theme.Web.Pages.Outbound;

public class SalesOrdersEditModel : PageModel
{
    private readonly ILogger<SalesOrdersEditModel> _logger;

    public SalesOrdersEditModel(ILogger<SalesOrdersEditModel> logger)
    {
        _logger = logger;
    }

    [BindProperty]
    public EditSalesOrderInput Input { get; set; } = new();

    public List<WarehouseDTO> Warehouses { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public int SalesOrderId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        SalesOrderId = id;
        await Task.Yield();

        // Mock sales order
        var saleOrder = new SaleOrder
        {
            Id = id,
            Code = "SO-MOCK-00" + id,
            WarehouseId = 1,
            Warehouse = new Wms.Theme.Domain.Entities.Warehouse { Id = 1, Name = "Mock Warehouse", Code = "WH-MOCK", Address = "Mock Address" },
            Customer = "Mock Customer",
            OrderDate = DateTime.Now,
            ExpectedDeliveryDate = DateTime.Now.AddDays(3)
        };


        // Populate form
        Input = new EditSalesOrderInput
        {
            WarehouseId = saleOrder.WarehouseId,
            Code = saleOrder.Code,
            Customer = saleOrder.Customer,
            OrderDate = saleOrder.OrderDate,
            ExpectedDeliveryDate = saleOrder.ExpectedDeliveryDate
        };

        // Load list of warehouses
        await LoadWarehouses();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        SalesOrderId = id;

        if (!ModelState.IsValid)
        {
            await LoadWarehouses();
            return Page();
        }

        try
        {
            await Task.Yield();

            // Mock success
            _logger.LogInformation("Sales order {Code} updated successfully (Mock)", Input.Code);

            // Redirect to the sales orders list with success message
            TempData["SuccessMessage"] = $"Đơn bán hàng '{Input.Code}' đã được cập nhật thành công.";
            return RedirectToPage("/Outbound/SalesOrders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating sales order");
            ErrorMessage = "An unexpected error occurred. Please try again.";
            await LoadWarehouses();
            return Page();
        }
    }

    private async Task LoadWarehouses()
    {
        await Task.Yield();
        Warehouses = new List<WarehouseDTO>
        {
             new() { Id = 1, Name = "Main Warehouse", Code = "WH-001" },
             new() { Id = 2, Name = "South Branch", Code = "WH-002" },
             new() { Id = 3, Name = "East Distribution", Code = "WH-003" }
        };
    }
}

public class EditSalesOrderInput
{
    [Required(ErrorMessage = "Warehouse is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid warehouse")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "SO Code is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "SO Code must be between 3 and 50 characters")]
    public string Code { get; set; } = "";

    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 100 characters")]
    public string Customer { get; set; } = "";

    [Required(ErrorMessage = "Order date is required")]
    public DateTime OrderDate { get; set; }

    [Display(Name = "Expected Delivery Date")]
    public DateTime? ExpectedDeliveryDate { get; set; }
}
